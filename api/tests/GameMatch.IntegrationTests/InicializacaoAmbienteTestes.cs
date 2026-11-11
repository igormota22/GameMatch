using System.Runtime.CompilerServices;

namespace GameMatch.IntegrationTests;

internal static class InicializacaoAmbienteTestes
{
    /// <summary>
    /// Chave de assinatura usada apenas nos testes. Nunca deve ser reutilizada
    /// fora do ambiente de teste.
    /// </summary>
    private const string ChaveJwtTeste =
        "gamematch-chave-de-teste-nao-usar-fora-dos-testes";

    [ModuleInitializer]
    internal static void Inicializar()
    {
        if (string.IsNullOrWhiteSpace(
                Environment.GetEnvironmentVariable("ConnectionStrings__GameMatch")))
        {
            Environment.SetEnvironmentVariable(
                "ConnectionStrings__GameMatch",
                "Host=localhost;Port=5432;Database=gamematch_teste;Username=teste;Password=teste");
        }

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("Jwt__Chave")))
        {
            Environment.SetEnvironmentVariable("Jwt__Chave", ChaveJwtTeste);
        }
    }
}
