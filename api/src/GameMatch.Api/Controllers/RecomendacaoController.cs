using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameMatch.Application.Recomendacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameMatch.Api.Controllers;

/// <summary>
/// Expõe as recomendações personalizadas e as opções do Modo Descoberta sob
/// sessão autenticada, com ProblemDetails identificável para 404 e 409.
/// </summary>
[ApiController]
[Route("recomendacoes")]
[Authorize]
public sealed class RecomendacaoController(ServicoRecomendacoes servico)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<RespostaRecomendacoes>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Obter(
        [FromQuery] string? experiencia,
        CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();

        if (usuarioId is null)
        {
            return SessaoInvalida();
        }

        var resultado = await servico.ExecutarAsync(
            usuarioId,
            experiencia,
            cancellationToken);

        if (resultado.Sucesso)
        {
            return Ok(new RespostaRecomendacoes(resultado.Itens!));
        }

        return resultado.CodigoErro switch
        {
            ResultadoRecomendacoes.ErroPerfilIncompleto => Falha(
                StatusCodes.Status409Conflict,
                ResultadoRecomendacoes.ErroPerfilIncompleto,
                "Informe gêneros e plataformas no perfil para receber " +
                "recomendações.",
                resultado.Erros),

            _ => Falha(
                StatusCodes.Status404NotFound,
                ResultadoRecomendacoes.ErroExperienciaDesconhecida,
                "Use uma das opções de experiência de /recomendacoes/opcoes.",
                resultado.Erros)
        };
    }

    [HttpGet("opcoes")]
    [ProducesResponseType<OpcaoDescobertaDto[]>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public IActionResult ObterOpcoes() => Ok(servico.ListarOpcoes());

    private string? ObterUsuarioId() =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub);

    private ObjectResult SessaoInvalida() =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Sessão inválida",
            detail: "Não foi possível identificar o usuário autenticado.");

    private ObjectResult Falha(
        int statusCode,
        string title,
        string detail,
        IReadOnlyDictionary<string, string[]> erros) =>
        Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            extensions: new Dictionary<string, object?>
            {
                ["errors"] = erros
            });
}
