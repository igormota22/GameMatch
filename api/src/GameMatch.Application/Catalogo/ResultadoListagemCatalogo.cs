namespace GameMatch.Application.Catalogo;

/// <summary>
/// Resultado da listagem do catálogo: página válida ou erros de validação dos
/// parâmetros de consulta.
/// </summary>
public sealed record ResultadoListagemCatalogo(
    PaginaDto<JogoResumoDto>? Pagina,
    IReadOnlyDictionary<string, string[]> Erros)
{
    public bool Sucesso => Erros.Count == 0;

    public static ResultadoListagemCatalogo Ok(PaginaDto<JogoResumoDto> pagina) =>
        new(pagina, new Dictionary<string, string[]>());

    public static ResultadoListagemCatalogo Invalido(
        IReadOnlyDictionary<string, string[]> erros) =>
        new(null, erros);
}
