namespace GameMatch.Application.Perfil;

/// <summary>
/// Representação de leitura do perfil gamer. O campo <see cref="Completo"/> é
/// calculado pela regra de domínio e não pode ser enviado pelo cliente.
/// </summary>
public sealed record PerfilDto(
    IReadOnlyList<string> Generos,
    IReadOnlyList<string> Plataformas,
    IReadOnlyList<string> Estilos,
    IReadOnlyList<string> Experiencias,
    bool Completo,
    DateTimeOffset? AtualizadoEm);
