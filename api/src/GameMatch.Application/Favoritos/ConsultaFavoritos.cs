namespace GameMatch.Application.Favoritos;

/// <summary>
/// Consulta da lista de favoritos com paginação opcional. Não há filtros: a
/// lista reúne apenas os favoritos do próprio usuário (RF-18).
/// </summary>
public sealed record ConsultaFavoritos(
    int Pagina = ConsultaFavoritos.PaginaPadrao,
    int Tamanho = ConsultaFavoritos.TamanhoPadrao)
{
    public const int PaginaPadrao = 1;

    public const int TamanhoPadrao = 12;

    public const int TamanhoMaximo = 100;

    public static ConsultaFavoritos Criar(int? pagina, int? tamanho) =>
        new(
            pagina ?? PaginaPadrao,
            tamanho ?? TamanhoPadrao);

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
}
