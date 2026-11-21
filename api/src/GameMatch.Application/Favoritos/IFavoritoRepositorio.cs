using GameMatch.Domain.Entidades;

namespace GameMatch.Application.Favoritos;

/// <summary>
/// Contrato de persistência dos favoritos. A infraestrutura implementa a
/// idempotência da criação e a remoção por par (INV-05).
/// </summary>
public interface IFavoritoRepositorio
{
    Task<bool> JogoExisteAsync(
        int jogoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona o par (usuário, jogo) sem duplicar. Devolve <c>true</c> quando
    /// o favorito foi criado e <c>false</c> quando já existia.
    /// </summary>
    Task<bool> AdicionarAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove o favorito do par. Devolve <c>false</c> quando não existia.
    /// </summary>
    Task<bool> RemoverAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default);

    Task<PaginaFavoritos> ListarAsync(
        string usuarioId,
        ConsultaFavoritos consulta,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> ObterIdsAsync(
        string usuarioId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Página de favoritos com os jogos da página e o total do usuário antes da
/// paginação.
/// </summary>
public sealed record PaginaFavoritos(
    IReadOnlyList<Jogo> Jogos,
    int Total);
