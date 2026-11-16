namespace GameMatch.Application.Perfil;

/// <summary>
/// Resultado do caso de uso de salvamento: ou o perfil atualizado, ou os erros
/// de validação por campo, sem efeito no banco.
/// </summary>
public sealed record ResultadoSalvarPerfil(
    PerfilDto? Perfil,
    IReadOnlyDictionary<string, string[]> Erros)
{
    public bool Sucesso => Perfil is not null;

    public static ResultadoSalvarPerfil Ok(PerfilDto perfil) =>
        new(perfil, new Dictionary<string, string[]>());

    public static ResultadoSalvarPerfil Falha(
        IReadOnlyDictionary<string, string[]> erros) =>
        new(null, erros);
}
