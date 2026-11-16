using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameMatch.Application.Perfil;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameMatch.Api.Controllers;

[ApiController]
[Route("perfil")]
[Authorize]
public sealed class PerfilController(ServicoPerfil servicoPerfil) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PerfilDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Obter(CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
        {
            return SessaoInvalida();
        }

        return Ok(await servicoPerfil.ObterAsync(usuarioId, cancellationToken));
    }

    [HttpPut]
    [ProducesResponseType<PerfilDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Salvar(
        [FromBody] PerfilSalvarDto requisicao,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
        {
            return SessaoInvalida();
        }

        var resultado = await servicoPerfil.SalvarAsync(
            usuarioId,
            requisicao,
            cancellationToken);

        if (!resultado.Sucesso)
        {
            return Problem(
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Perfil inválido",
                detail: "Revise as preferências informadas.",
                extensions: new Dictionary<string, object?>
                {
                    ["errors"] = resultado.Erros
                });
        }

        return Ok(resultado.Perfil);
    }

    private string? ObterUsuarioId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    private ObjectResult SessaoInvalida() =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Sessão inválida",
            detail: "Não foi possível identificar o usuário autenticado.");
}
