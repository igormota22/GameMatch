using System.Runtime.CompilerServices;

namespace GameMatch.IntegrationTests;

internal static class InicializacaoAmbienteTestes
{
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
    }
}
