using GameMatch.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameMatch.Infrastructure;

public static class ExtensaoServicosInfraestrutura
{
    public static IServiceCollection AdicionarInfraestrutura(
        this IServiceCollection services,
        string stringConexao)
    {
        if (string.IsNullOrWhiteSpace(stringConexao))
        {
            throw new ArgumentException(
                "A string de conexão do banco não pode ser vazia.",
                nameof(stringConexao));
        }

        services.AddDbContext<GameMatchDbContext>(opcoes =>
            opcoes.UseNpgsql(stringConexao));

        return services;
    }
}
