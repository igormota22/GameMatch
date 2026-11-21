namespace GameMatch.Application.Favoritos;

/// <summary>
/// Resultado da listagem de favoritos: página válida ou erros de paginação.
/// </summary>
public sealed record ResultadoListagemFavoritos(
    Catalogo.PaginaDto<Catalogo.JogoResumoDto>? Pagina,
    IReadOnlyDictionary<string, string[]> Erros)
{
    public bool Sucesso => Erros.Count == 0;

    public static ResultadoListagemFavoritos Ok(
        Catalogo.PaginaDto<Catalogo.JogoResumoDto> pagina) =>
        new(pagina, new Dictionary<string, string[]>());

    public static ResultadoListagemFavoritos Invalido(
        IReadOnlyDictionary<string, string[]> erros) =>
        new(null, erros);
}

/// <summary>
/// Resultado de favoritar: criado, já existente (idempotente) ou jogo
/// inexistente no catálogo.
/// </summary>
public enum ResultadoFavoritar
{
    Criado,
    JaExistente,
    JogoNaoEncontrado
}

/// <summary>
/// Contrato de saída de <c>GET /favoritos/ids</c>, usado pela interface para
/// refletir o estado dos cards (D1 do design).
/// </summary>
public sealed record FavoritosIdsDto(IReadOnlyList<int> Ids);
