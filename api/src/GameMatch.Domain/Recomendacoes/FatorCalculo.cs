namespace GameMatch.Domain.Recomendacoes;

/// <summary>
/// Um fator da pontuação entregue para a explicação ao usuário: a chave e o
/// rótulo exibidos, quanto ele contribuiu e quais itens corresponderam.
/// </summary>
public sealed record FatorCalculo(
    string Chave,
    string Rotulo,
    int Contribuicao,
    IReadOnlyList<string> Correspondencias);
