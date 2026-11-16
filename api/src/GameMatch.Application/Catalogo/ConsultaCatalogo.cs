namespace GameMatch.Application.Catalogo;

/// <summary>
/// Consulta do catálogo com paginação, busca e filtros combináveis. Valores
/// vazios são tratados como ausentes.
/// </summary>
public sealed record ConsultaCatalogo(
    int Pagina = ConsultaCatalogo.PaginaPadrao,
    int Tamanho = ConsultaCatalogo.TamanhoPadrao,
    string? Busca = null,
    string? Genero = null,
    string? Plataforma = null,
    string? Tag = null)
{
    public const int PaginaPadrao = 1;

    public const int TamanhoPadrao = 24;

    public const int TamanhoMaximo = 100;

    public static ConsultaCatalogo Criar(
        int? pagina,
        int? tamanho,
        string? busca,
        string? genero,
        string? plataforma,
        string? tag) =>
        new(
            pagina ?? PaginaPadrao,
            tamanho ?? TamanhoPadrao,
            Limpar(busca),
            Limpar(genero),
            Limpar(plataforma),
            Limpar(tag));

    public IReadOnlyDictionary<string, string[]> Validar()
    {
        var erros = new Dictionary<string, string[]>();

        if (Pagina < 1)
        {
            erros["pagina"] = ["A página deve ser maior ou igual a 1."];
        }

        if (Tamanho < 1 || Tamanho > TamanhoMaximo)
        {
            erros["tamanho"] = [$"O tamanho deve estar entre 1 e {TamanhoMaximo}."];
        }

        return erros;
    }

    private static string? Limpar(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
