using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using GameMatch.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace GameMatch.IntegrationTests;

/// <summary>
/// Sobe um PostgreSQL efêmero via Testcontainers. Quando o Docker não está
/// disponível, usa ou cria um banco dedicado de testes.
/// </summary>
public sealed class FabricaApiComBanco : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    public string StringConexao { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        try
        {
            _container = new PostgreSqlBuilder("postgres:17-alpine")
                .WithDatabase("gamematch_teste")
                .WithUsername("gamematch_teste")
                .WithPassword("gamematch_teste")
                .Build();

            await _container.StartAsync();
            StringConexao = _container.GetConnectionString();
        }
        catch (Exception excecao)
        {
            Console.WriteLine(
                $"Testcontainers indisponível ({excecao.Message}). " +
                "Usando banco de teste dedicado.");

            StringConexao = await ObterOuCriarBancoDedicadoAsync();
        }

        Environment.SetEnvironmentVariable("ConnectionStrings__GameMatch", StringConexao);

        using var escopo = Services.CreateScope();
        var contexto = escopo.ServiceProvider.GetRequiredService<GameMatchDbContext>();
        await contexto.Database.MigrateAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    private static async Task<string> ObterOuCriarBancoDedicadoAsync()
    {
        var configurada = Environment.GetEnvironmentVariable("ConnectionStrings__GameMatchTeste");
        if (!string.IsNullOrWhiteSpace(configurada))
        {
            return configurada;
        }

        const string bancoTeste = "gamematch_teste";
        var administrador = new NpgsqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("ConnectionStrings__GameMatch")
            ?? "Host=localhost;Port=5432;Database=gamematch;Username=gamematch;Password=gamematch-dev")
        {
            Database = "postgres"
        };

        await using (var conexao = new NpgsqlConnection(administrador.ConnectionString))
        {
            await conexao.OpenAsync();

            await using var comando = new NpgsqlCommand(
                $"CREATE DATABASE \"{bancoTeste}\"",
                conexao);

            try
            {
                await comando.ExecuteNonQueryAsync();
            }
            catch (PostgresException excecao) when (excecao.SqlState == "42P04")
            {
                // O banco dedicado já existe.
            }
        }

        administrador.Database = bancoTeste;
        return administrador.ConnectionString;
    }
}
