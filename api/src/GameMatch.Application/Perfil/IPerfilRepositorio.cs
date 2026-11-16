using GameMatch.Domain.Entidades;

namespace GameMatch.Application.Perfil;

/// <summary>
/// Contrato de persistência do perfil. A implementação vive na infraestrutura
/// e o caso de uso depende apenas desta abstração.
/// </summary>
public interface IPerfilRepositorio
{
    Task<PerfilGamer?> ObterAsync(
        string usuarioId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria o perfil ou substitui integralmente as preferências existentes.
    /// </summary>
    Task SubstituirAsync(
        PerfilGamer perfil,
        CancellationToken cancellationToken = default);
}
