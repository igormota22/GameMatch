namespace GameMatch.Application.Recomendacoes;

/// <summary>
/// Opções de recomendação lidas da seção `Recomendacoes:*` da configuração
/// (D3 do design, sem `IOptions`). <see cref="Top"/> é o tamanho do topo
/// devolvido (DT-14).
/// </summary>
public sealed class OpcoesRecomendacoes
{
    public const int TopPadrao = 12;

    public int Top { get; set; } = TopPadrao;
}
