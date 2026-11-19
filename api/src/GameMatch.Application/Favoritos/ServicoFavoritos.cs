using GameMatch.Application.Catalogo;

namespace GameMatch.Application.Favoritos;

/// <summary>
/// Caso de uso dos favoritos: favoritar de forma idempotente, remover,
/// listar com paginação e devolver os ids marcados para a interface
/// (RF-16 e RF-18; INV-05).
/// </summary>
public sealed class ServicoFavoritos(IFavoritoRepositorio repositorio)
{
    public async Task<ResultadoFavoritar> AdicionarAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default)
    {
        if (!await repositorio.JogoExisteAsync(jogoId, cancellationToken))
        {
            return ResultadoFavoritar.JogoNaoEncontrado;
        }

        var criado = await repositorio.AdicionarAsync(
            usuarioId,
            jogoId,
            cancellationToken);

        return criado
            ? ResultadoFavoritar.Criado
            : ResultadoFavoritar.JaExistente;
    }

    public Task<bool> RemoverAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default) =>
        repositorio.RemoverAsync(usuarioId, jogoId, cancellationToken);

    public async Task<ResultadoListagemFavoritos> ListarAsync(
        string usuarioId,
        ConsultaFavoritos consulta,
        CancellationToken cancellationToken = default)
    {
        var erros = consulta.Validar();

        if (erros.Count > 0)
        {
            return ResultadoListagemFavoritos.Invalido(erros);
        }

        var resultado = await repositorio.ListarAsync(
            usuarioId,
            consulta,
            cancellationToken);

        return ResultadoListagemFavoritos.Ok(
            new PaginaDto<JogoResumoDto>(
                resultado.Jogos.Select(MapeadorJogoResumo.Mapear).ToArray(),
                consulta.Pagina,
                consulta.Tamanho,
                resultado.Total));
    }

    public async Task<FavoritosIdsDto> ObterIdsAsync(
        string usuarioId,
        CancellationToken cancellationToken = default) =>
        new(await repositorio.ObterIdsAsync(usuarioId, cancellationToken));
}
