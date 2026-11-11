using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace GameMatch.IntegrationTests;

public sealed class FabricaApi : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureServices(servicos =>
        {
            servicos
                .AddControllers()
                .AddApplicationPart(typeof(ErroController).Assembly);
        });
    }
}
