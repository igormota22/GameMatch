namespace GameMatch.Domain.Recomendacoes;

/// <summary>
/// Preferências do jogador usadas no cálculo de compatibilidade. Os grupos
/// vêm do perfil gamer e mantêm a grafia canônica do catálogo.
/// </summary>
public sealed record PreferenciasCalculo(
    IReadOnlyList<string> Generos,
    IReadOnlyList<string> Plataformas,
    IReadOnlyList<string> Estilos);
