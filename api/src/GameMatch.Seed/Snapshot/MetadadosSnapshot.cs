using System.Text.Json.Serialization;

namespace GameMatch.Seed.Snapshot;

/// <summary>
/// Metadados da coleta do snapshot, gravados junto dos dados.
/// </summary>
public sealed record MetadadosSnapshot(
    [property: JsonPropertyName("coletadoEm")] DateTimeOffset ColetadoEm,
    [property: JsonPropertyName("totalJogos")] int TotalJogos,
    [property: JsonPropertyName("totalTags")] int TotalTags,
    [property: JsonPropertyName("tagsIgnoradas")] IReadOnlyList<string> TagsIgnoradas,
    [property: JsonPropertyName("origem")] string Origem,
    [property: JsonPropertyName("versaoFormato")] int VersaoFormato)
{
    public const int VersaoFormatoAtual = 1;
}

/// <summary>
/// Conteúdo do arquivo <c>tags.json</c>: mapa de tag para identificadores
/// externos dos jogos que a possuem.
/// </summary>
public sealed class ArquivoTags
{
    [JsonPropertyName("tags")]
    public Dictionary<string, int[]> Tags { get; set; } = new();
}
