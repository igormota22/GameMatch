namespace GameMatch.Application.Avaliacoes;

/// <summary>
/// Avaliação do próprio usuário para um jogo, exposta com a nota de 1 a 5
/// estrelas (RF-17).
/// </summary>
public sealed record AvaliacaoDto(int Nota);
