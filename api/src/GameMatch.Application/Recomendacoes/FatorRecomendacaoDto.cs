namespace GameMatch.Application.Recomendacoes;

/// <summary>
/// Fator explicado de uma recomendação: chave e rótulo exibidos, quanto ele
/// contribuiu para a pontuação e quais itens corresponderam (RF-11).
/// </summary>
public sealed record FatorRecomendacaoDto(
    string Chave,
    string Rotulo,
    int Contribuicao,
    IReadOnlyList<string> Correspondencias);
