using GameMatch.Application.Catalogo;
using GameMatch.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.Infrastructure.Persistencia.Repositorios;

/// <summary>
/// Consultas do catálogo em PostgreSQL. A busca usa ILIKE com o termo escapado
/// e os filtros combinam características por igualdade.
/// </summary>
public sealed class CatalogoRepositorio(GameMatchDbContext contexto) : ICatalogoRepositorio
{
    public async Task<PaginaCatalogo> ListarAsync(
        ConsultaCatalogo consulta,
        CancellationToken cancellationToken = default)
    {
        var jogos = AplicarFiltros(contexto.Jogos.AsNoTracking(), consulta);

        var total = await jogos.CountAsync(cancellationToken);
        var pulo = (long)(consulta.Pagina - 1) * consulta.Tamanho;

        if (pulo >= total)
        {
            return new PaginaCatalogo([], total);
        }

        var pagina = await jogos
            .OrderBy(jogo => jogo.Titulo)
            .ThenBy(jogo => jogo.Id)
            .Skip((int)pulo)
            .Take(consulta.Tamanho)
            .Include(jogo => jogo.Caracteristicas)
            .AsSplitQuery()
            .ToArrayAsync(cancellationToken);

        return new PaginaCatalogo(pagina, total);
    }

    public async Task<FiltrosCatalogoDto> ObterFiltrosAsync(
        CancellationToken cancellationToken = default)
    {
        var valores = (await contexto.JogosCaracteristicas
            .AsNoTracking()
            .Select(caracteristica => new
            {
                caracteristica.Tipo,
                caracteristica.Valor
            })
            .Distinct()
            .ToListAsync(cancellationToken))
            .Select(valor => (valor.Tipo, valor.Valor))
            .ToArray();

        return new FiltrosCatalogoDto(
            Ordenar(valores, TipoCaracteristica.Genero),
            Ordenar(valores, TipoCaracteristica.Plataforma),
            Ordenar(valores, TipoCaracteristica.Estilo));
    }

    public Task<Jogo?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default) =>
        contexto.Jogos
            .AsNoTracking()
            .Include(jogo => jogo.Caracteristicas)
            .FirstOrDefaultAsync(jogo => jogo.Id == id, cancellationToken);

    private static IQueryable<Jogo> AplicarFiltros(
        IQueryable<Jogo> jogos,
        ConsultaCatalogo consulta)
    {
        if (consulta.Busca is not null)
        {
            var termo = EscaparLike(consulta.Busca);

            jogos = jogos.Where(jogo =>
                EF.Functions.ILike(jogo.Titulo, $"%{termo}%"));
        }

        if (consulta.Genero is not null)
        {
            jogos = ComCaracteristica(jogos, TipoCaracteristica.Genero, consulta.Genero);
        }

        if (consulta.Plataforma is not null)
        {
            jogos = ComCaracteristica(
                jogos,
                TipoCaracteristica.Plataforma,
                consulta.Plataforma);
        }

        if (consulta.Tag is not null)
        {
            jogos = ComCaracteristica(jogos, TipoCaracteristica.Estilo, consulta.Tag);
        }

        return jogos;
    }

    private static IQueryable<Jogo> ComCaracteristica(
        IQueryable<Jogo> jogos,
        TipoCaracteristica tipo,
        string valor) =>
        jogos.Where(jogo => jogo.Caracteristicas
            .Any(caracteristica =>
                caracteristica.Tipo == tipo && caracteristica.Valor == valor));

    private static string EscaparLike(string termo) =>
        termo
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");

    private static string[] Ordenar(
        IEnumerable<(TipoCaracteristica Tipo, string Valor)> valores,
        TipoCaracteristica tipo) =>
        valores
            .Where(valor => valor.Tipo == tipo)
            .Select(valor => valor.Valor)
            .OrderBy(valor => valor, StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
