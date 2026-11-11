using System.Net;
using System.Text;
using System.Text.Json;

namespace GameMatch.IntegrationTests;

public sealed class ProblemDetailsTests : IClassFixture<FabricaApi>
{
    private readonly FabricaApi _fabrica;

    public ProblemDetailsTests(FabricaApi fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Falha_inesperada_retorna_problemdetails_sem_stack_trace()
    {
        using var cliente = _fabrica.CreateClient();
        using var resposta = await cliente.GetAsync("/api/teste/erro");

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(corpo);

        Assert.Equal(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.True(json.RootElement.TryGetProperty("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));
        Assert.DoesNotContain("Falha simulada", corpo);
        Assert.DoesNotContain("InvalidOperationException", corpo);
        Assert.DoesNotContain("StackTrace", corpo, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Requisicao_invalida_retorna_problemdetails_400()
    {
        using var cliente = _fabrica.CreateClient();
        using var conteudo = new StringContent("{}", Encoding.UTF8, "application/json");
        using var resposta = await cliente.PostAsync("/api/teste/validacao", conteudo);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(corpo);

        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        Assert.True(json.RootElement.TryGetProperty("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));
        Assert.True(json.RootElement.TryGetProperty("errors", out var erros));
        Assert.True(erros.TryGetProperty("Nome", out _));
    }

    [Fact]
    public async Task Rota_inexistente_retorna_problemdetails_com_traceid()
    {
        using var cliente = _fabrica.CreateClient();
        using var resposta = await cliente.GetAsync("/api/rota-que-nao-existe");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(corpo);

        Assert.Equal(404, json.RootElement.GetProperty("status").GetInt32());
        Assert.True(json.RootElement.TryGetProperty("traceId", out var traceId));
        Assert.False(string.IsNullOrWhiteSpace(traceId.GetString()));
    }
}
