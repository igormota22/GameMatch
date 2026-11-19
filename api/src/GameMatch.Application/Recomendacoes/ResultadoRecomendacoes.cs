namespace GameMatch.Application.Recomendacoes;

/// <summary>
/// Resultado do caso de uso: ou a lista de recomendações, ou a falha
/// identificável pelo controlador — perfil incompleto (409) ou experiência
/// desconhecida (404) — sem usar exceções (D4 do design).
/// </summary>
public sealed record ResultadoRecomendacoes(
    IReadOnlyList<RecomendacaoDto>? Itens,
    string? CodigoErro,
    IReadOnlyDictionary<string, string[]> Erros)
{
    public const string ErroPerfilIncompleto = "perfil-incompleto";
    public const string ErroExperienciaDesconhecida = "experiencia-desconhecida";

    public bool Sucesso => Itens is not null;

    public static ResultadoRecomendacoes Ok(IReadOnlyList<RecomendacaoDto> itens) =>
        new(itens, null, new Dictionary<string, string[]>());

    public static ResultadoRecomendacoes PerfilIncompleto() =>
        new(
            null,
            ErroPerfilIncompleto,
            new Dictionary<string, string[]>
            {
                ["perfil"] =
                [
                    "Informe ao menos um gênero e uma plataforma para " +
                    "receber recomendações."
                ]
            });

    public static ResultadoRecomendacoes ExperienciaDesconhecida(string chave) =>
        new(
            null,
            ErroExperienciaDesconhecida,
            new Dictionary<string, string[]>
            {
                ["experiencia"] =
                [$"'{chave}' não é uma opção válida do Modo Descoberta."]
            });
}
