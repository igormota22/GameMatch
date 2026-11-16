namespace GameMatch.Application.Catalogo;

/// <summary>
/// Opções de filtro disponíveis no catálogo sincronizado.
/// </summary>
public sealed record FiltrosCatalogoDto(
    IReadOnlyList<string> Generos,
    IReadOnlyList<string> Plataformas,
    IReadOnlyList<string> Tags);
