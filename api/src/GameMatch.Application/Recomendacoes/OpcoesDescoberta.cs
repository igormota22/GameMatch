namespace GameMatch.Application.Recomendacoes;

/// <summary>
/// Opções do Modo Descoberta lidas da seção `Descoberta:Opcoes` (DT-09),
/// preservando a ordem configurada.
/// </summary>
public sealed class OpcoesDescoberta
{
    public List<OpcaoDescoberta> Opcoes { get; set; } = [];

    /// <summary>
    /// Localiza a opção pela chave enviada na consulta, sem diferenciar
    /// maiúsculas de minúsculas; devolve nulo quando a chave é desconhecida.
    /// </summary>
    public OpcaoDescoberta? PorChave(string chave) =>
        Opcoes.FirstOrDefault(opcao =>
            string.Equals(opcao.Chave, chave, StringComparison.OrdinalIgnoreCase));
}
