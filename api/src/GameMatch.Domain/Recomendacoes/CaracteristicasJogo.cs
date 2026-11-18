namespace GameMatch.Domain.Recomendacoes;

/// <summary>
/// Características de um jogo do catálogo usadas no cálculo. Os estilos do
/// jogo são as tags sincronizadas pela fonte.
/// </summary>
public sealed record CaracteristicasJogo(
    IReadOnlyList<string> Generos,
    IReadOnlyList<string> Plataformas,
    IReadOnlyList<string> Tags);
