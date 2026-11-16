using System.Text.Encodings.Web;
using System.Text.Json;
using GameMatch.Seed.Coleta;

namespace GameMatch.Seed.Snapshot;

/// <summary>
/// Grava o snapshot de forma atômica por arquivo: o conteúdo é escrito em um
/// temporário e movido sobre o destino só depois de completo. Os metadados são
/// gravados por último e sinalizam o fim da coleta.
/// </summary>
public static class EscritorSnapshot
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task SalvarAsync(
        string diretorio,
        IReadOnlyList<JogoDaApi> jogos,
        IReadOnlyDictionary<string, int[]> tags,
        MetadadosSnapshot metadados,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(diretorio);

        await EscreverAsync(
            diretorio,
            "tags.json",
            new ArquivoTags { Tags = new Dictionary<string, int[]>(tags) },
            cancellationToken);

        await EscreverAsync(diretorio, "jogos.json", jogos, cancellationToken);
        await EscreverAsync(diretorio, "metadados.json", metadados, cancellationToken);
    }

    private static async Task EscreverAsync<T>(
        string diretorio,
        string nome,
        T conteudo,
        CancellationToken cancellationToken)
    {
        var destino = Path.Combine(diretorio, nome);
        var temporario = $"{destino}.tmp";

        await using (var fluxo = File.Create(temporario))
        {
            await JsonSerializer.SerializeAsync(
                fluxo,
                conteudo,
                Json,
                cancellationToken);
        }

        File.Move(temporario, destino, overwrite: true);
    }
}
