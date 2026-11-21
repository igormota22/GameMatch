using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameMatch.Application.Favoritos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameMatch.Api.Controllers;

/// <summary>
/// Expõe os favoritos do usuário autenticado: listagem paginada, favoritar de
/// forma idempotente (201/200), remoção (204) e os ids marcados para o estado
/// dos cards (D1 do design da change).
/// </summary>
[ApiController]
[Route("favoritos")]
[Authorize]
public sealed class FavoritoController(ServicoFavoritos servico) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<Application.Catalogo.PaginaDto<Application.Catalogo.JogoResumoDto>>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Listar(
        [FromQuery] int? pagina,
        [FromQuery] int? tamanho,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
        {
            return SessaoInvalida();
        }

        var resultado = await servico.ListarAsync(
            usuarioId,
            ConsultaFavoritos.Criar(pagina, tamanho),
            cancellationToken);

        if (!resultado.Sucesso)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Consulta inválida",
                detail: "Revise os parâmetros de paginação.",
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = resultado.Erros
                });
        }

        return Ok(resultado.Pagina);
    }

    [HttpGet("ids")]
    [ProducesResponseType<FavoritosIdsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterIds(CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
        {
            return SessaoInvalida();
        }

        return Ok(await servico.ObterIdsAsync(usuarioId, cancellationToken));
    }

    [HttpPost("{jogoId:int}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Adicionar(
        int jogoId,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
        {
            return SessaoInvalida();
        }

        var resultado = await servico.AdicionarAsync(
            usuarioId,
            jogoId,
            cancellationToken);

        return resultado switch
        {
            ResultadoFavoritar.Criado => StatusCode(StatusCodes.Status201Created),
            ResultadoFavoritar.JaExistente => Ok(),
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

        var removido = await servico.RemoverAsync(
            usuarioId,
            jogoId,
            cancellationToken);

        return removido ? NoContent() : FavoritoNaoEncontrado(jogoId);
    }

    private string? ObterUsuarioId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    private ObjectResult JogoNaoEncontrado(int jogoId) =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Jogo não encontrado",
            detail: $"Nenhum jogo do catálogo possui o identificador {jogoId}.");

    private ObjectResult FavoritoNaoEncontrado(int jogoId) =>
        Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Favorito não encontrado",
            detail: $"O jogo {jogoId} não está nos favoritos do usuário.");

    private ObjectResult SessaoInvalida() =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Sessão inválida",
            detail: "Não foi possível identificar o usuário autenticado.");
}
