namespace GameMatch.Domain.Entidades;

/// <summary>
/// Tipos de preferência aceitos no perfil e no catálogo. Os valores numéricos
/// são persistidos no banco e não devem ser reordenados sem uma migração.
/// </summary>
public enum TipoPreferencia
{
    Genero = 1,
    Plataforma = 2,
    Estilo = 3,
    Experiencia = 4
}
