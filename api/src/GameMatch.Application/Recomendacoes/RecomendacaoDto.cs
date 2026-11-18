using GameMatch.Application.Catalogo;

namespace GameMatch.Application.Recomendacoes;

/// <summary>
/// Recomendação explicável: o resumo do jogo, a pontuação de compatibilidade
/// de 0 a 100 e os fatores que a compõem (RF-10 e RF-11).
/// </summary>
public sealed record RecomendacaoDto(
    JogoResumoDto Jogo,
    int Pontuacao,
    IReadOnlyList<FatorRecomendacaoDto> Fatores);
