
using GameMatch.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GameMatch.Api.Diagnostico;

/// <summary>
/// Verifica a conectividade com o PostgreSQL por meio do DbContext da aplicação.
/// </summary>
public sealed class VerificacaoSaudePostgres(IServiceScopeFactory escopoFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var escopo = escopoFactory.CreateAsyncScope();

            var dbContext = escopo.ServiceProvider
                .GetRequiredService<GameMatchDbContext>();

            await dbContext.Database.OpenConnectionAsync(cancellationToken);
            await dbContext.Database.CloseConnectionAsync();

            return HealthCheckResult.Healthy(
                "Conexão com o PostgreSQL bem-sucedida.");
        }
        catch (Exception excecao)
        {
            return HealthCheckResult.Unhealthy(
                "Não foi possível conectar ao PostgreSQL.",
                excecao);
        }
    }
}
