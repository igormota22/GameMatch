using GameMatch.Domain.Entidades;

namespace GameMatch.Application.Catalogo;

/// <summary>
/// Caso de uso do catálogo: valida a consulta, pagina os resultados e projeta
/// os jogos nos DTOs de resumo e de detalhe.
/// </summary>
public sealed class ServicoCatalogo(ICatalogoRepositorio repositorio)
{
    public async Task<ResultadoListagemCatalogo> ListarAsync(
        ConsultaCatalogo consulta,
        CancellationToken cancellationToken = default)
    {
        var erros = consulta.Validar();

        if (erros.Count > 0)
        {
            return ResultadoListagemCatalogo.Invalido(erros);
        }

        var resultado = await repositorio.ListarAsync(consulta, cancellationToken);

        return ResultadoListagemCatalogo.Ok(
            new PaginaDto<JogoResumoDto>(
                resultado.Jogos.Select(MapearResumo).ToArray(),
                consulta.Pagina,
                consulta.Tamanho,
                resultado.Total));
    }

    public Task<FiltrosCatalogoDto> ObterFiltrosAsync(
        CancellationToken cancellationToken = default) =>
        repositorio.ObterFiltrosAsync(cancellationToken);

    public async Task<JogoDetalheDto?> ObterAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var jogo = await repositorio.ObterPorIdAsync(id, cancellationToken);

        return jogo is null ? null : MapearDetalhe(jogo);
    }

    private static JogoResumoDto MapearResumo(Jogo jogo) =>
        new(
            jogo.Id,
            jogo.Titulo,
            jogo.Thumbnail,
            Valores(jogo, TipoCaracteristica.Genero),
            Valores(jogo, TipoCaracteristica.Plataforma),
            jogo.Desenvolvedor);

    private static JogoDetalheDto MapearDetalhe(Jogo jogo) =>
        new(
            jogo.Id,
            jogo.Titulo,
            jogo.Descricao,
            jogo.Thumbnail,
            jogo.Desenvolvedor,
            jogo.UrlFonte,
            Valores(jogo, TipoCaracteristica.Genero),
            Valores(jogo, TipoCaracteristica.Plataforma),
            Valores(jogo, TipoCaracteristica.Estilo));

    private static string[] Valores(Jogo jogo, TipoCaracteristica tipo) =>
        jogo.Caracteristicas
            .Where(caracteristica => caracteristica.Tipo == tipo)
            .Select(caracteristica => caracteristica.Valor)
            .OrderBy(valor => valor, StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
