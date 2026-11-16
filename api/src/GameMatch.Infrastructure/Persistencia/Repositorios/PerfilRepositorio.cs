using GameMatch.Application.Perfil;
using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.Infrastructure.Persistencia.Repositorios;

/// <summary>
/// Persistência do perfil gamer em PostgreSQL. A substituição apaga as
/// preferências atuais e grava o novo conjunto dentro de uma transação.
/// </summary>
public sealed class PerfilRepositorio(GameMatchDbContext contexto) : IPerfilRepositorio
{
    public Task<PerfilGamer?> ObterAsync(
        string usuarioId,
        CancellationToken cancellationToken = default) =>
        contexto.PerfisGamer
            .AsNoTracking()
            .Include(perfil => perfil.Preferencias)
            .FirstOrDefaultAsync(
                perfil => perfil.UsuarioId == usuarioId,
                cancellationToken);

    public async Task SubstituirAsync(
        PerfilGamer perfil,
        CancellationToken cancellationToken = default)
    {
        await using var transacao = await contexto.Database
            .BeginTransactionAsync(cancellationToken);

        var existente = await contexto.PerfisGamer
            .FirstOrDefaultAsync(
                registro => registro.UsuarioId == perfil.UsuarioId,
                cancellationToken);

        if (existente is null)
        {
            contexto.PerfisGamer.Add(perfil);
        }
        else
        {
            await contexto.PerfisPreferencias
                .Where(preferencia => preferencia.UsuarioId == perfil.UsuarioId)
                .ExecuteDeleteAsync(cancellationToken);

            existente.AtualizadoEm = perfil.AtualizadoEm;
            contexto.PerfisPreferencias.AddRange(perfil.Preferencias);
        }

        await contexto.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
    }
}
