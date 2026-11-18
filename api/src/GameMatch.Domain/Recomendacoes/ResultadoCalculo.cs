namespace GameMatch.Domain.Recomendacoes;

/// <summary>
/// Resultado do cálculo para um jogo: pontuação inteira de 0 a 100 e os
/// fatores que a explicam, na ordem em que são exibidos.
/// </summary>
public sealed record ResultadoCalculo(
    int Pontuacao,
    IReadOnlyList<FatorCalculo> Fatores);
