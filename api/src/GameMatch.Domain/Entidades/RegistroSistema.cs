namespace GameMatch.Domain.Entidades;

/// <summary>
/// Metadados de infraestrutura do sistema, usados para versionar o schema e
/// registrar o estado da carga inicial do catálogo.
/// </summary>
public sealed class RegistroSistema
{
    public string Chave { get; set; } = string.Empty;

    public string Valor { get; set; } = string.Empty;

    public DateTimeOffset AtualizadoEm { get; set; }
}
