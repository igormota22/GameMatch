namespace GameMatch.Domain.Entidades;

/// <summary>
/// Preferência de um jogador para um tipo de característica. A chave composta
/// por usuário, tipo e valor garante que a mesma preferência não seja
/// persistida duas vezes.
/// </summary>
public sealed class PreferenciaPerfil
{
    public string UsuarioId { get; set; } = string.Empty;

    public TipoPreferencia Tipo { get; set; }

    public string Valor { get; set; } = string.Empty;
}
