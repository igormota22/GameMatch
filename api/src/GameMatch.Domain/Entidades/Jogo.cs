namespace GameMatch.Domain.Entidades;

/// <summary>
/// Jogo do catálogo sincronizado a partir do snapshot da FreeToGame. O
/// identificador externo é único e sustenta o upsert da sincronização.
/// </summary>
public sealed class Jogo
{
    public int Id { get; set; }

    public string IdExterno { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public string Thumbnail { get; set; } = string.Empty;

    public string Desenvolvedor { get; set; } = string.Empty;

    public string UrlFonte { get; set; } = string.Empty;

    public DateTimeOffset SincronizadoEm { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<JogoCaracteristica> Caracteristicas { get; set; } =
        new List<JogoCaracteristica>();
}
