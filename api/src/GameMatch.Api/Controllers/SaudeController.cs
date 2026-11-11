using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GameMatch.Api.Controllers;

[ApiController]
[Route("health")]
[AllowAnonymous]
public sealed class SaudeController(HealthCheckService servicoSaude) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ObterSaude(CancellationToken cancellationToken)
    {
        var relatorio = await servicoSaude.CheckHealthAsync(cancellationToken);

        var resposta = new
        {
            status = relatorio.Status.ToString(),
            duracaoTotalMs = relatorio.TotalDuration.TotalMilliseconds,
            verificacoes = relatorio.Entries.ToDictionary(
                entrada => entrada.Key,
                entrada => new
                {
                    status = entrada.Value.Status.ToString(),
                    descricao = entrada.Value.Description,
                    erro = entrada.Value.Exception?.Message,
                    duracaoMs = entrada.Value.Duration.TotalMilliseconds
                })
        };

        var codigoHttp = relatorio.Status == HealthStatus.Healthy
            ? StatusCodes.Status200OK
            : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(codigoHttp, resposta);
    }
}
