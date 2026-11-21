using GameMatch.Domain.Entidades;

namespace GameMatch.Application.Avaliacoes;

/// <summary>
/// Contrato de persistência das avaliações. A infraestrutura implementa o
/// upsert na PK composta e a remoção por par (INV-04/INV-05).
/// </summary>
public interface IAvaliacaoRepositorio
{
    Task<bool> JogoExisteAsync(
        int jogoId,
        CancellationToken cancellationToken = default);

    Task<Avaliacao?> ObterAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria a avaliação ou substitui a nota existente do par (usuário, jogo),
    /// mantendo um único registro por jogo (RB-15).
    /// </summary>
    Task SalvarAsync(
        string usuarioId,
        int jogoId,
        int nota,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a avaliação do par. Devolve <c>false</c> quando não existia.
    /// </summary>
    Task<bool> RemoverAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default);
}
