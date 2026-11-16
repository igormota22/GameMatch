namespace GameMatch.Application.Catalogo;

/// <summary>
/// Página de resultados de catálogo com o total de itens que satisfazem a
/// consulta antes da paginação.
/// </summary>
public sealed record PaginaDto<T>(
    IReadOnlyList<T> Items,
    int Pagina,
    int Tamanho,
    int Total);
