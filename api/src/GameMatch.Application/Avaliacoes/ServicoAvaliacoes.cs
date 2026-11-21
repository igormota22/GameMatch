using GameMatch.Domain.Entidades;

namespace GameMatch.Application.Avaliacoes;

/// <summary>
/// Caso de uso das avaliações: valida a nota antes de tocar no banco, cria ou
/// substitui a avaliação (uma por jogo), consulta e remove (RF-17, RB-15;
/// INV-04/INV-05).
/// </summary>
public sealed class ServicoAvaliacoes(IAvaliacaoRepositorio repositorio)
{
    public async Task<AvaliacaoDto?> ObterAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default)
    {
        var avaliacao = await repositorio.ObterAsync(
            usuarioId,
            jogoId,
            cancellationToken);

        return avaliacao is null ? null : new AvaliacaoDto(avaliacao.Nota);
    }

    public async Task<ResultadoSalvarAvaliacao> SalvarAsync(
        string usuarioId,
        int jogoId,
        int nota,
        CancellationToken cancellationToken = default)
    {
        if (!Avaliacao.ValidarNota(nota))
        {
            return ResultadoSalvarAvaliacao.ForaDaFaixa();
        }

        if (!await repositorio.JogoExisteAsync(jogoId, cancellationToken))
        {
            return ResultadoSalvarAvaliacao.JogoInexistente();
        }

        await repositorio.SalvarAsync(
            usuarioId,
            jogoId,
            nota,
            cancellationToken);

        return ResultadoSalvarAvaliacao.Ok(nota);
    }

    public Task<bool> RemoverAsync(
        string usuarioId,
        int jogoId,
        CancellationToken cancellationToken = default) =>
        repositorio.RemoverAsync(usuarioId, jogoId, cancellationToken);
}
