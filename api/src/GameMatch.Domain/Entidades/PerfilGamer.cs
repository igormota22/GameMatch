namespace GameMatch.Domain.Entidades;

/// <summary>
/// Perfil gamer de um usuário. É 1:1 com a conta e mantém as preferências
/// normalizadas que alimentarão o cálculo de compatibilidade.
/// </summary>
public sealed class PerfilGamer
{
    public string UsuarioId { get; set; } = string.Empty;

    public DateTimeOffset AtualizadoEm { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<PreferenciaPerfil> Preferencias { get; set; } =
        new List<PreferenciaPerfil>();

    /// <summary>
    /// O perfil só está completo com ao menos um gênero e uma plataforma;
    /// estilos e experiências são opcionais.
    /// </summary>
    public bool EstaCompleto =>
        Preferencias.Any(preferencia => preferencia.Tipo == TipoPreferencia.Genero) &&
        Preferencias.Any(preferencia => preferencia.Tipo == TipoPreferencia.Plataforma);
}
