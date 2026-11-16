using GameMatch.Application.Catalogo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameMatch.Api.Controllers;

[ApiController]
[Route("catalogo")]
[Authorize]
public sealed class CatalogoController(ServicoCatalogo servicoCatalogo) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PaginaDto<JogoResumoDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Listar(
        [FromQuery] int? pagina,
        [FromQuery] int? tamanho,
        [FromQuery] string? busca,
        [FromQuery] string? genero,
        [FromQuery] string? plataforma,
        [FromQuery] string? tag,
        CancellationToken cancellationToken)
    {
        var consulta = ConsultaCatalogo.Criar(
            pagina,
            tamanho,
            busca,
            genero,
            plataforma,
            tag);

        var resultado = await servicoCatalogo.ListarAsync(consulta, cancellationToken);

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

    [HttpGet("filtros")]
    [ProducesResponseType<FiltrosCatalogoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ObterFiltros(CancellationToken cancellationToken) =>
        Ok(await servicoCatalogo.ObterFiltrosAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [ProducesResponseType<JogoDetalheDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var jogo = await servicoCatalogo.ObterAsync(id, cancellationToken);

        if (jogo is null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Jogo não encontrado",
                detail: $"Nenhum jogo do catálogo possui o identificador {id}.");
        }

        return Ok(jogo);
    }
}
