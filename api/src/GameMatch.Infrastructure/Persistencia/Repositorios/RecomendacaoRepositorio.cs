using GameMatch.Application.Recomendacoes;
using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.Infrastructure.Persistencia.Repositorios;

/// <summary>
/// Leituras das recomendações em PostgreSQL: as preferências do usuário e o
/// catálogo completo com características agrupadas por tipo, sem
/// rastreamento, para o cálculo em memória (D2 do design).
/// </summary>
public sealed class RecomendacaoRepositorio(GameMatchDbContext contexto)
    : IRecomendacaoRepositorio
{
    public Task<PerfilGamer?> ObterPreferenciasAsync(
        string usuarioId,
        CancellationToken cancellationToken = default) =>
        contexto.PerfisGamer
            .AsNoTracking()
            .Include(perfil => perfil.Preferencias)
            .FirstOrDefaultAsync(
                perfil => perfil.UsuarioId == usuarioId,
                cancellationToken);

    public async Task<IReadOnlyList<JogoRecomendacao>> ObterCatalogoAsync(
        CancellationToken cancellationToken = default)
    {
        // Uma única leitura em lote: o catálogo completo já com os valores
        // separados em gêneros, plataformas e tags (estilos), na ordem do id.
        var jogos = await contexto.Jogos
            .AsNoTracking()
            .Select(jogo => new
            {
                jogo.Id,
                jogo.Titulo,
                jogo.Descricao,
                jogo.Thumbnail,
                jogo.Desenvolvedor,
                Generos = jogo.Caracteristicas
                    .Where(caracteristica =>
                        caracteristica.Tipo == TipoCaracteristica.Genero)
                    .Select(caracteristica => caracteristica.Valor)
                    .ToList(),
                Plataformas = jogo.Caracteristicas
                    .Where(caracteristica =>
                        caracteristica.Tipo == TipoCaracteristica.Plataforma)
                    .Select(caracteristica => caracteristica.Valor)
                    .ToList(),
                Tags = jogo.Caracteristicas
                    .Where(caracteristica =>
                        caracteristica.Tipo == TipoCaracteristica.Estilo)
                    .Select(caracteristica => caracteristica.Valor)
                    .ToList()
            })
            .OrderBy(jogo => jogo.Id)
            .ToListAsync(cancellationToken);

        return jogos
            .Select(jogo => new JogoRecomendacao(
                jogo.Id,
                jogo.Titulo,
                jogo.Descricao,
                jogo.Thumbnail,
                jogo.Desenvolvedor,
                jogo.Generos,
                jogo.Plataformas,
                jogo.Tags))
            .ToArray();
    }
}
