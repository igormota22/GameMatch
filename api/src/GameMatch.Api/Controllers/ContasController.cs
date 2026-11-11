using GameMatch.Api.Autenticacao;
using GameMatch.Api.Contratos;
using GameMatch.Domain.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GameMatch.Api.Controllers;

[ApiController]
[Route("contas")]
[AllowAnonymous]
public sealed class ContasController(
    UserManager<Usuario> gerenciadorUsuarios,
    SignInManager<Usuario> gerenciadorLogin,
    GeradorTokenJwt geradorToken) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<RespostaAutenticacao>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroRequisicao requisicao)
    {
        var usuario = new Usuario
        {
            UserName = requisicao.Email,
            Email = requisicao.Email
        };

        var resultado = await gerenciadorUsuarios.CreateAsync(usuario, requisicao.Senha);

        if (!resultado.Succeeded)
        {
            if (MensagensIdentidade.ContemErroDeDuplicidade(resultado.Errors))
            {
                return Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "E-mail já cadastrado",
                    detail: "Já existe uma conta com este e-mail. Tente entrar ou use outro e-mail.");
            }

            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Não foi possível criar a conta",
                detail: string.Join(" ", MensagensIdentidade.Traduzir(resultado.Errors)));
        }

        return StatusCode(StatusCodes.Status201Created, CriarResposta(usuario));
    }

    [HttpPost("login")]
    [ProducesResponseType<RespostaAutenticacao>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Entrar([FromBody] LoginRequisicao requisicao)
    {
        var usuario = await gerenciadorUsuarios.FindByEmailAsync(requisicao.Email);

        if (usuario is null)
        {
            return CredenciaisInvalidas();
        }

        var resultado = await gerenciadorLogin.CheckPasswordSignInAsync(
            usuario,
            requisicao.Senha,
            lockoutOnFailure: true);

        if (!resultado.Succeeded)
        {
            return CredenciaisInvalidas();
        }

        return Ok(CriarResposta(usuario));
    }

    private RespostaAutenticacao CriarResposta(Usuario usuario)
    {
        var token = geradorToken.Gerar(usuario);

        return new RespostaAutenticacao(
            token.Token,
            token.ExpiraEm,
            new UsuarioResposta(usuario.Email ?? string.Empty, usuario.CriadoEm));
    }

    private ObjectResult CredenciaisInvalidas() =>
        Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Credenciais inválidas",
            detail: "E-mail ou senha incorretos.");
}
