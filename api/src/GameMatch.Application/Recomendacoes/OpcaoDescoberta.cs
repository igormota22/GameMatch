namespace GameMatch.Application.Recomendacoes;

/// <summary>
/// Opção de experiência do Modo Descoberta (DT-09): chave enviada em
/// `experiencia`, rótulo exibido no seletor e o mapeamento (gêneros e tags do
/// catálogo) que alimenta o fator de experiência na pontuação.
/// </summary>
public sealed class OpcaoDescoberta
{
    public string Chave { get; set; } = string.Empty;

    public string Rotulo { get; set; } = string.Empty;

    public List<string> Generos { get; set; } = [];

    public List<string> Tags { get; set; } = [];
}
