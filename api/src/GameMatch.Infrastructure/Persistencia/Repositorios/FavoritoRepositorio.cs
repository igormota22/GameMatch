using GameMatch.Application.Favoritos;
using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.Infrastructure.Persistencia.Repositorios;

/// <summary>
/// Persistência dos favoritos em PostgreSQL. A criação usa
/// <c>ON CONFLICT DO NOTHING</c> para manter a idempotência mesmo com
/// requisições concorrentes (INV-05) e a listagem ordena pelos mais recentes.
/// </summary>
public sealed class FavoritoRepositorio(GameMatchDbContext contexto)
    : IFavoritoRepositorio
{
    public Task<bool> JogoExisteAsync(
        int jogoId,
        CancellationToken cancellationToken = default) =>
        contexto.Jogos.AnyAsync(jogo => jogo.Id == jogoId, cancellationToken);

    public async Task<bool> AdicionarAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default)
    {
        var linhas = await contexto.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO favoritos (usuario_id, jogo_id, criado_em)
            VALUES ({usuarioId}, {jogoId}, {DateTimeOffset.UtcNow})
            ON CONFLICT (usuario_id, jogo_id) DO NOTHING
            """,
            cancellationToken);

        return linhas > 0;
    }

    public async Task<bool> RemoverAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default)
    {
        var linhas = await contexto.Favoritos
            .Where(favorito =>
                favorito.UsuarioId == usuarioId && favorito.JogoId == jogoId)
            .ExecuteDeleteAsync(cancellationToken);

        return linhas > 0;
    }

    public async Task<PaginaFavoritos> ListarAsync(
        string usuarioId,
        ConsultaFavoritos consulta,
        CancellationToken cancellationToken = default)
    {
        var favoritos = contexto.Favoritos
            .AsNoTracking()
            .Where(favorito => favorito.UsuarioId == usuarioId)
            .OrderByDescending(favorito => favorito.CriadoEm)
            .ThenBy(favorito => favorito.JogoId);

        var total = await favoritos.CountAsync(cancellationToken);
        var pulo = (long)(consulta.Pagina - 1) * consulta.Tamanho;

        if (pulo >= total)
        {
            return new PaginaFavoritos([], total);
        }

        var ids = await favoritos
            .Skip((int)pulo)
            .Take(consulta.Tamanho)
            .Select(favorito => favorito.JogoId)
            .ToArrayAsync(cancellationToken);

        var jogos = await contexto.Jogos
            .AsNoTracking()
            .Include(jogo => jogo.Caracteristicas)
            .Where(jogo => ids.Contains(jogo.Id))
            .AsSplitQuery()
            .ToArrayAsync(cancellationToken);

        var porId = jogos.ToDictionary(jogo => jogo.Id);
        var pagina = ids
            .Where(porId.ContainsKey)
            .Select(id => porId[id])
            .ToArray();

        return new PaginaFavoritos(pagina, total);
    }

    public async Task<IReadOnlyList<int>> ObterIdsAsync(
        string usuarioId,
        CancellationToken cancellationToken = default) =>
        await contexto.Favoritos
            .AsNoTracking()
            .Where(favorito => favorito.UsuarioId == usuarioId)
            .Select(favorito => favorito.JogoId)
            .OrderBy(jogoId => jogoId)
            .ToArrayAsync(cancellationToken);
}
