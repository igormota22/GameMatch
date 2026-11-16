using System.Globalization;
using System.Text.Json;

namespace GameMatch.Seed.Snapshot;

/// <summary>
/// Lê e valida os três arquivos do snapshot antes de qualquer escrita no banco.
/// </summary>
public sealed class LeitorSnapshot
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public SnapshotCatalogo Ler(string diretorio)
    {
        var jogos = LerArquivo<List<Coleta.JogoDaApi>>(diretorio, "jogos.json");
        var arquivoTags = LerArquivo<ArquivoTags>(diretorio, "tags.json");
        var metadados = LerArquivo<MetadadosSnapshot>(diretorio, "metadados.json");

        ValidarJogos(jogos);

        var tags = ValidarTags(arquivoTags);

        ValidarMetadados(metadados, jogos.Count, tags.Count);

        return new SnapshotCatalogo(
            jogos.Select(jogo => new JogoSnapshot(
                jogo.Id,
                jogo.Id.ToString(CultureInfo.InvariantCulture),
                jogo.Title!.Trim(),
                jogo.ShortDescription ?? string.Empty,
                jogo.Thumbnail ?? string.Empty,
                jogo.Developer ?? string.Empty,
                jogo.FreeToGameProfileUrl ?? jogo.GameUrl ?? string.Empty,
                jogo.Genre!.Trim(),
                jogo.Platform!.Trim()))
                .ToArray(),
            tags,
            metadados);
    }

    private static T LerArquivo<T>(string diretorio, string nome)
    {
        var caminho = Path.Combine(diretorio, nome);

        if (!File.Exists(caminho))
        {
            throw new FormatoSnapshotInvalidoException(
                $"Arquivo do snapshot não encontrado: {caminho}");
        }

        try
        {
            var conteudo = File.ReadAllText(caminho);
            var valor = JsonSerializer.Deserialize<T>(conteudo, Json);

            return valor ?? throw new FormatoSnapshotInvalidoException(
                $"Arquivo do snapshot vazio: {nome}");
        }
        catch (JsonException excecao)
        {
            throw new FormatoSnapshotInvalidoException(
                $"JSON inválido em {nome}: {excecao.Message}");
        }
    }

    private static void ValidarJogos(List<Coleta.JogoDaApi> jogos)
    {
        if (jogos.Count == 0)
        {
            throw new FormatoSnapshotInvalidoException(
                "O snapshot não contém jogos.");
        }

        var duplicados = jogos
            .GroupBy(jogo => jogo.Id)
            .Where(grupo => grupo.Count() > 1)
            .Select(grupo => grupo.Key)
            .ToArray();

        if (duplicados.Length > 0)
        {
            throw new FormatoSnapshotInvalidoException(
                "Identificador externo duplicado no snapshot: " +
                string.Join(", ", duplicados));
        }

        foreach (var jogo in jogos)
        {
            if (jogo.Id <= 0 || string.IsNullOrWhiteSpace(jogo.Title))
            {
                throw new FormatoSnapshotInvalidoException(
                    "Todo jogo do snapshot precisa de identificador e título.");
            }

            if (string.IsNullOrWhiteSpace(jogo.Genre) ||
                string.IsNullOrWhiteSpace(jogo.Platform))
            {
                throw new FormatoSnapshotInvalidoException(
                    $"O jogo '{jogo.Title}' não possui gênero ou plataforma.");
            }
        }
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ValidarTags(
        ArquivoTags arquivo)
    {
        var tags = new Dictionary<string, IReadOnlyList<string>>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var (tag, ids) in arquivo.Tags)
        {
            var nome = tag.Trim();

            if (nome.Length == 0)
            {
                throw new FormatoSnapshotInvalidoException(
                    "O snapshot contém uma tag sem nome.");
            }

            var valores = ids
                .Where(id => id > 0)
                .Distinct()
                .OrderBy(id => id)
                .Select(id => id.ToString(CultureInfo.InvariantCulture))
                .ToArray();

            if (!tags.TryAdd(nome, valores))
            {
                throw new FormatoSnapshotInvalidoException(
                    $"Tag duplicada no snapshot: {nome}");
            }
        }

        return tags;
    }

    private static void ValidarMetadados(
        MetadadosSnapshot metadados,
        int totalJogos,
        int totalTags)
    {
        if (metadados.ColetadoEm == default)
        {
            throw new FormatoSnapshotInvalidoException(
                "Os metadados do snapshot não informam a data de coleta.");
        }

        if (metadados.VersaoFormato != MetadadosSnapshot.VersaoFormatoAtual)
        {
            throw new FormatoSnapshotInvalidoException(
                $"Versão de formato do snapshot não suportada: " +
                $"{metadados.VersaoFormato}.");
        }

        if (metadados.TotalJogos != totalJogos)
        {
            throw new FormatoSnapshotInvalidoException(
                "Os metadados não correspondem ao arquivo de jogos.");
        }

        if (metadados.TotalTags != totalTags)
        {
            throw new FormatoSnapshotInvalidoException(
                "Os metadados não correspondem ao arquivo de tags.");
        }
    }
}
