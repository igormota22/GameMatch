namespace GameMatch.Domain.Entidades;

/// <summary>
/// Tipos de característica do catálogo. Os valores numéricos são persistidos no
/// banco e espelham os primeiros valores de <see cref="TipoPreferencia"/>; não
/// devem ser reordenados sem uma migração.
/// </summary>
public enum TipoCaracteristica
{
    Genero = 1,
    Plataforma = 2,
    Estilo = 3
}
