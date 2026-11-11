using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using GameMatch.Api.Autenticacao;
using GameMatch.Api.Contratos;
using GameMatch.Domain.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GameMatch.Api.Controllers;

[ApiController]
[Route("conta")]
[Authorize]
public sealed class ContaController(UserManager<Usuario> gerenciadorUsuarios) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<UsuarioResposta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Obter()
    {
        var usuario = await ObterUsuarioAtualAsync();

        if (usuario is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Sessão inválida",
                detail: "Não foi possível identificar o usuário autenticado.");
        }

        return Ok(new UsuarioResposta(usuario.Email ?? string.Empty, usuario.CriadoEm));
    }

    [HttpPut("senha")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AlterarSenha([FromBody] AlteracaoSenhaRequisicao requisicao)
    {
        var usuario = await ObterUsuarioAtualAsync();

        if (usuario is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Sessão inválida",
                detail: "Não foi possível identificar o usuário autenticado.");
        }

        var resultado = await gerenciadorUsuarios.ChangePasswordAsync(
            usuario,
            requisicao.SenhaAtual,
            requisicao.NovaSenha);

        if (!resultado.Succeeded)
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Não foi possível alterar a senha",
                detail: string.Join(" ", MensagensIdentidade.Traduzir(resultado.Errors)));
        }

        return NoContent();
    }

    private Task<Usuario?> ObterUsuarioAtualAsync()
    {
        var identificador = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return string.IsNullOrWhiteSpace(identificador)
            ? Task.FromResult<Usuario?>(null)
            : gerenciadorUsuarios.FindByIdAsync(identificador);
    }
}
