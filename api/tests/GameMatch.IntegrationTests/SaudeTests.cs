using System.Net;

namespace GameMatch.IntegrationTests;

public sealed class SaudeTests : IClassFixture<FabricaApiComBanco>
{
    private readonly FabricaApiComBanco _fabrica;

    public SaudeTests(FabricaApiComBanco fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Health_retorna_healthy_com_banco_disponivel()
    {
        using var cliente = _fabrica.CreateClient();
        using var resposta = await cliente.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", corpo);
        Assert.Contains("postgresql", corpo);
    }
}
