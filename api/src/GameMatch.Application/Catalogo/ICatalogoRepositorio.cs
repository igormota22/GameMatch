using GameMatch.Domain.Entidades;

namespace GameMatch.Application.Catalogo;

/// <summary>
/// Contrato de consulta do catálogo. A implementação vive na infraestrutura e o
/// caso de uso depende apenas desta abstração.
/// </summary>
public interface ICatalogoRepositorio
{
    Task<PaginaCatalogo> ListarAsync(
        ConsultaCatalogo consulta,
        CancellationToken cancellationToken = default);

    Task<FiltrosCatalogoDto> ObterFiltrosAsync(
        CancellationToken cancellationToken = default);

    Task<Jogo?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado da listagem com os jogos da página e o total antes da paginação.
/// </summary>
public sealed record PaginaCatalogo(
    IReadOnlyList<Jogo> Jogos,
    int Total);
