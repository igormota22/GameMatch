using System.Net.Http.Json;
using System.Text.Json;
using GameMatch.Seed.Snapshot;

namespace GameMatch.Seed.Coleta;

/// <summary>
/// Coleta o snapshot do catálogo a partir da API pública da FreeToGame. Falhas
/// em tags isoladas não interrompem a coleta; falha na lista de jogos, sim.
/// </summary>
public sealed class ColetorSnapshot(HttpClient http)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<ResumoColeta> ColetarAsync(
        OpcoesColeta opcoes,
        string diretorio,
        Action<string>? aviso = null,
        CancellationToken cancellationToken = default)
    {
        http.BaseAddress ??= new Uri(opcoes.BaseUrl.TrimEnd('/') + "/");
        http.Timeout = TimeSpan.FromSeconds(opcoes.TimeoutSegundos);
        http.DefaultRequestHeaders.UserAgent.Clear();
        http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "GameMatch.Seed/1.0 (+https://github.com/igormota22/GameMatch)");

        var jogos = await http.GetFromJsonAsync<List<JogoDaApi>>(
            "games",
            Json,
            cancellationToken) ?? [];

        if (jogos.Count == 0)
        {
            throw new InvalidOperationException(
                "A fonte não retornou jogos para o snapshot.");
        }

        var invalido = jogos.FirstOrDefault(jogo =>
            jogo.Id <= 0 || string.IsNullOrWhiteSpace(jogo.Title));

        if (invalido is not null)
        {
            throw new InvalidOperationException(
                "A fonte retornou um jogo sem identificador ou título.");
        }

        var tags = new SortedDictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
        var ignoradas = new List<string>();

        foreach (var tag in opcoes.TagsCandidatas
                     .Where(tag => !string.IsNullOrWhiteSpace(tag))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var encontrados = await ColetarTagAsync(tag, ignoradas, aviso, cancellationToken);

            if (encontrados is { Length: > 0 })
            {
                tags[tag] = encontrados;
            }
        }

        var metadados = new MetadadosSnapshot(
            DateTimeOffset.UtcNow,
            jogos.Count,
            tags.Count,
            ignoradas,
            opcoes.BaseUrl,
            MetadadosSnapshot.VersaoFormatoAtual);

        await EscritorSnapshot.SalvarAsync(diretorio, jogos, tags, metadados, cancellationToken);

        return new ResumoColeta(
            jogos.Count,
            tags.Count,
            ignoradas,
            metadados.ColetadoEm);
    }

    private async Task<int[]?> ColetarTagAsync(
        string tag,
        ICollection<string> ignoradas,
        Action<string>? aviso,
        CancellationToken cancellationToken)
    {
        var caminho = $"filter?tag={Uri.EscapeDataString(tag)}";

        try
        {
            var resposta = await http.GetAsync(caminho, cancellationToken);

            if (!resposta.IsSuccessStatusCode)
            {
                ignoradas.Add(tag);
                aviso?.Invoke(
                    $"tag '{tag}' ignorada: a fonte respondeu {(int)resposta.StatusCode}.");
                return null;
            }

            var encontrados = await resposta.Content.ReadFromJsonAsync<List<JogoDaApi>>(
                Json,
                cancellationToken) ?? [];

            if (encontrados.Count == 0)
            {
                ignoradas.Add(tag);
                aviso?.Invoke($"tag '{tag}' ignorada: nenhum jogo retornado.");
                return null;
            }

            return encontrados
                .Select(jogo => jogo.Id)
                .Where(id => id > 0)
                .Distinct()
                .OrderBy(id => id)
                .ToArray();
        }
        catch (Exception excecao) when (excecao is HttpRequestException or JsonException)
        {
            ignoradas.Add(tag);
            aviso?.Invoke($"tag '{tag}' ignorada: {excecao.Message}");
            return null;
        }
    }
}

/// <summary>
/// Resumo da coleta exibido pelo comando.
/// </summary>
public sealed record ResumoColeta(
    int TotalJogos,
    int TotalTags,
    IReadOnlyList<string> TagsIgnoradas,
    DateTimeOffset ColetadoEm);
