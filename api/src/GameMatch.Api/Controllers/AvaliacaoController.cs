using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameMatch.Application.Avaliacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameMatch.Api.Controllers;

/// <summary>
/// Expõe a avaliação do próprio usuário autenticado para um jogo: consulta
/// (404 quando ausente), upsert de 1 a 5 estrelas (422 fora da faixa, 404 para
/// jogo inexistente) e remoção (204/404) — §5.1 do TDD.
/// </summary>
[ApiController]
[Route("avaliacoes")]
[Authorize]
public sealed class AvaliacaoController(ServicoAvaliacoes servico)
    : ControllerBase
{
    [HttpGet("{jogoId:int}")]
    [ProducesResponseType<AvaliacaoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(
        int jogoId,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
        {
            return SessaoInvalida();
        }

        var avaliacao = await servico.ObterAsync(
            usuarioId,
            jogoId,
            cancellationToken);

        return avaliacao is null ? AvaliacaoNaoEncontrada(jogoId) : Ok(avaliacao);
    }

    [HttpPut("{jogoId:int}")]
    [ProducesResponseType<AvaliacaoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Salvar(
        int jogoId,
        [FromBody] AvaliacaoSalvarDto entrada,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
        {
            return SessaoInvalida();
        }

        var resultado = await servico.SalvarAsync(
            usuarioId,
            jogoId,
            entrada.Nota,
            cancellationToken);

        return resultado.Status switch
        {
            StatusSalvarAvaliacao.Salvo => Ok(resultado.Avaliacao),
            StatusSalvarAvaliacao.NotaInvalida => NotaForaDaFaixa(),
            _ => JogoNaoEncontrado(jogoId)
        };
    }

    [HttpDelete("{jogoId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(
        int jogoId,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
        {
            return SessaoInvalida();
        }

        var removida = await servico.RemoverAsync(
            usuarioId,
            jogoId,
            cancellationToken);

        return removida ? NoContent() : AvaliacaoNaoEncontrada(jogoId);
    }

    private string? ObterUsuarioId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    private ObjectResult NotaForaDaFaixa() =>
        Problem(
            statusCode: StatusCodes.Status422UnprocessableEntity,
            title: "Nota inválida",
            detail: "A nota deve ser um inteiro de 1 a 5 estrelas.");

    private ObjectResult JogoNaoEncontrado(int jogoId) =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Jogo não encontrado",
            detail: $"Nenhum jogo do catálogo possui o identificador {jogoId}.");

    private ObjectResult AvaliacaoNaoEncontrada(int jogoId) =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Avaliação não encontrada",
            detail: $"O usuário não avaliou o jogo {jogoId}.");

    private ObjectResult SessaoInvalida() =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Sessão inválida",
            detail: "Não foi possível identificar o usuário autenticado.");
}

/// <summary>
/// Corpo de <c>PUT /avaliacoes/{jogoId}</c> com a nota inteira de 1 a 5.
/// </summary>
public sealed record AvaliacaoSalvarDto(int Nota);
