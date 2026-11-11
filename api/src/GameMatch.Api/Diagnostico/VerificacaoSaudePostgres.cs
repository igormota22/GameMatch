using GameMatch.Infrastructure.Persistencia;
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
            var dbContext = escopo.ServiceProvider.GetRequiredService<GameMatchDbContext>();

            var conectado = await dbContext.Database.CanConnectAsync(cancellationToken);

            return conectado
                ? HealthCheckResult.Healthy("Conexão com o PostgreSQL bem-sucedida.")
                : HealthCheckResult.Unhealthy("Não foi possível conectar ao PostgreSQL.");
        }
        catch (Exception excecao)
        {
            return HealthCheckResult.Unhealthy(
                "Não foi possível conectar ao PostgreSQL.",
                excecao);
        }
    }
}
