namespace GameMatch.Application.Recomendacoes;

/// <summary>
/// Envelope de `GET /recomendacoes`: as recomendações do topo ordenadas pela
/// pontuação (DT-14).
/// </summary>
public sealed record RespostaRecomendacoes(IReadOnlyList<RecomendacaoDto> Itens);
