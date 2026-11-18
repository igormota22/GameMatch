namespace GameMatch.Application.Perfil;

/// <summary>
/// Entrada do salvamento do perfil. Gêneros e plataformas são obrigatórios;
/// estilos podem vir vazios ou nulos.
/// </summary>
public sealed record PerfilSalvarDto(
    List<string>? Generos,
    List<string>? Plataformas,
    List<string>? Estilos);
