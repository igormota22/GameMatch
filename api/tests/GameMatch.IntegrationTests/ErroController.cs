using Microsoft.AspNetCore.Mvc;

namespace GameMatch.IntegrationTests;

/// <summary>
/// Controller exclusivo dos testes de integração, usado para simular uma falha
/// inesperada sem expor um endpoint de erro na aplicação.
/// </summary>
[ApiController]
[Route("api/teste")]
public sealed class ErroController : ControllerBase
{
    [HttpGet("erro")]
    public IActionResult GerarErro() =>
        throw new InvalidOperationException("Falha simulada para teste.");
}
