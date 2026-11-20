using GameMatch.Application.Avaliacoes;
using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.Infrastructure.Persistencia.Repositorios;

/// <summary>
/// Persistência das avaliações em PostgreSQL. O salvamento é um upsert na PK
/// composta, de modo que dois PUTs concorrentes convergem para um único
/// registro (D4/§7.2 do TDD).
/// </summary>
public sealed class AvaliacaoRepositorio(GameMatchDbContext contexto)
    : IAvaliacaoRepositorio
{
    public Task<bool> JogoExisteAsync(
        int jogoId,
        CancellationToken cancellationToken = default) =>
        contexto.Jogos.AnyAsync(jogo => jogo.Id == jogoId, cancellationToken);

    public Task<Avaliacao?> ObterAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default) =>
        contexto.Avaliacoes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                avaliacao =>
                    avaliacao.UsuarioId == usuarioId &&
                    avaliacao.JogoId == jogoId,
                cancellationToken);

    public async Task SalvarAsync(
        string usuarioId,
        int jogoId,
        int nota,
        CancellationToken cancellationToken = default)
    {
        await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO avaliacoes (usuario_id, jogo_id, nota, atualizada_em)
            VALUES ({usuarioId}, {jogoId}, {nota}, {DateTimeOffset.UtcNow})
            ON CONFLICT (usuario_id, jogo_id) DO UPDATE
            SET nota = EXCLUDED.nota,
                atualizada_em = EXCLUDED.atualizada_em
            """,
            cancellationToken);
    }

    public async Task<bool> RemoverAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default)
    {
        var linhas = await contexto.Avaliacoes
            .Where(avaliacao =>
                avaliacao.UsuarioId == usuarioId &&
                avaliacao.JogoId == jogoId)
            .ExecuteDeleteAsync(cancellationToken);

        return linhas > 0;
    }
}
