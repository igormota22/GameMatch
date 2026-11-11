using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace GameMatch.IntegrationTests;

public sealed class RequisicaoValidacao
{
    [Required]
    public string? Nome { get; set; }
}

/// <summary>
/// Controller exclusivo dos testes de integração, usado para exercitar a
/// validação automática do <c>[ApiController]</c> e o formato ProblemDetails
/// para requisições inválidas (400).
/// </summary>
[ApiController]
[Route("api/teste")]
public sealed class ValidacaoController : ControllerBase
{
    [HttpPost("validacao")]
    public IActionResult Validar([FromBody] RequisicaoValidacao requisicao) => Ok(requisicao);
}
