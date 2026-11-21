using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameMatch.Domain.Entidades;
using GameMatch.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.IntegrationTests;

/// <summary>
/// Cobertura de TC-07 para avaliações: 401, upsert 200, 422 fora da faixa,
/// 404 de jogo e de avaliação inexistente, remoção 204/404, isolamento entre
/// usuários e o cenário de concorrência da §7.2 (dois PUTs paralelos → um
/// único registro).
/// </summary>
public sealed class AvaliacaoTests : IClassFixture<FabricaApiComBanco>
{
    private const string SenhaValida = "Senha123";

    private readonly FabricaApiComBanco _fabrica;

    public AvaliacaoTests(FabricaApiComBanco fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Avaliacoes_sem_token_retornam_401()
    {
        using var cliente = _fabrica.CreateClient();

        using var consulta = await cliente.GetAsync("/avaliacoes/1");
        using var salvamento = await cliente.PutAsJsonAsync(
            "/avaliacoes/1",
            new { nota = 3 });
        using var remocao = await cliente.DeleteAsync("/avaliacoes/1");

        Assert.Equal(HttpStatusCode.Unauthorized, consulta.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, salvamento.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, remocao.StatusCode);
    }

    [Fact]
    public async Task Salvar_cria_a_avaliacao_e_consulta_devolve_a_nota()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var jogoId = await CriarJogoAsync("avaliacao-criacao");

        using var salvamento = await cliente.PutAsJsonAsync(
            $"/avaliacoes/{jogoId}",
            new { nota = 5 });

        Assert.Equal(HttpStatusCode.OK, salvamento.StatusCode);

        var avaliacao = await cliente.GetFromJsonAsync<AvaliacaoResposta>(
            $"/avaliacoes/{jogoId}");

        Assert.NotNull(avaliacao);
        Assert.Equal(5, avaliacao!.Nota);
        Assert.Equal(1, await ContarAvaliacoesAsync(jogoId));
    }

    [Fact]
    public async Task Salvar_novamente_substitui_a_nota_sem_duplicar()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var jogoId = await CriarJogoAsync("avaliacao-substituicao");

        await cliente.PutAsJsonAsync($"/avaliacoes/{jogoId}", new { nota = 2 });

        using var segundo = await cliente.PutAsJsonAsync(
            $"/avaliacoes/{jogoId}",
            new { nota = 4 });

        Assert.Equal(HttpStatusCode.OK, segundo.StatusCode);

        var avaliacao = await cliente.GetFromJsonAsync<AvaliacaoResposta>(
            $"/avaliacoes/{jogoId}");

        Assert.NotNull(avaliacao);
        Assert.Equal(4, avaliacao!.Nota);
        Assert.Equal(1, await ContarAvaliacoesAsync(jogoId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public async Task Nota_fora_da_faixa_retorna_422_sem_gravar(int nota)
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var jogoId = await CriarJogoAsync($"avaliacao-nota-{nota}");

        using var resposta = await cliente.PutAsJsonAsync(
            $"/avaliacoes/{jogoId}",
            new { nota });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            resposta.Content.Headers.ContentType?.MediaType);
        Assert.Equal(0, await ContarAvaliacoesAsync(jogoId));
    }

    [Fact]
    public async Task Avaliar_jogo_inexistente_retorna_404()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        using var resposta = await cliente.PutAsJsonAsync(
            "/avaliacoes/999999",
            new { nota = 3 });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            resposta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Consultar_avaliacao_inexistente_retorna_404()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var jogoId = await CriarJogoAsync("avaliacao-ausente");

        using var resposta = await cliente.GetAsync($"/avaliacoes/{jogoId}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Remover_avaliacao_devolve_204_e_consulta_seguinte_404()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var jogoId = await CriarJogoAsync("avaliacao-remocao");
        await cliente.PutAsJsonAsync($"/avaliacoes/{jogoId}", new { nota = 4 });

        using var remocao = await cliente.DeleteAsync($"/avaliacoes/{jogoId}");

        Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);

        using var consulta = await cliente.GetAsync($"/avaliacoes/{jogoId}");

        Assert.Equal(HttpStatusCode.NotFound, consulta.StatusCode);

        using var repeticao = await cliente.DeleteAsync($"/avaliacoes/{jogoId}");

        Assert.Equal(HttpStatusCode.NotFound, repeticao.StatusCode);
    }

    [Fact]
    public async Task Avaliacoes_de_usuarios_diferentes_ficam_isoladas()
    {
        using var clienteA = _fabrica.CreateClient();
        using var clienteB = _fabrica.CreateClient();

        await AutenticarAsync(clienteA, await CriarContaAsync(clienteA));
        await AutenticarAsync(clienteB, await CriarContaAsync(clienteB));

        var jogoId = await CriarJogoAsync("avaliacao-isolamento");

        await clienteA.PutAsJsonAsync($"/avaliacoes/{jogoId}", new { nota = 5 });

        using var consultaB = await clienteB.GetAsync($"/avaliacoes/{jogoId}");

        Assert.Equal(HttpStatusCode.NotFound, consultaB.StatusCode);

        // B avalia o mesmo jogo sem sobrescrever a nota de A (INV-07).
        await clienteB.PutAsJsonAsync($"/avaliacoes/{jogoId}", new { nota = 1 });

        var avaliacaoA = await clienteA.GetFromJsonAsync<AvaliacaoResposta>(
            $"/avaliacoes/{jogoId}");

        Assert.NotNull(avaliacaoA);
        Assert.Equal(5, avaliacaoA!.Nota);
        Assert.Equal(2, await ContarAvaliacoesAsync(jogoId));

        using var remocaoB = await clienteB.DeleteAsync($"/avaliacoes/{jogoId}");

        Assert.Equal(HttpStatusCode.NoContent, remocaoB.StatusCode);

        var avaliacaoAPosRemocaoB =
            await clienteA.GetFromJsonAsync<AvaliacaoResposta>(
                $"/avaliacoes/{jogoId}");

        Assert.NotNull(avaliacaoAPosRemocaoB);
        Assert.Equal(5, avaliacaoAPosRemocaoB!.Nota);
    }

    [Fact]
    public async Task Avaliacoes_em_paralelo_convergem_para_um_unico_registro()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var jogoId = await CriarJogoAsync("avaliacao-paralela");

        var respostas = await Task.WhenAll(
            cliente.PutAsJsonAsync($"/avaliacoes/{jogoId}", new { nota = 2 }),
            cliente.PutAsJsonAsync($"/avaliacoes/{jogoId}", new { nota = 4 }));

        Assert.All(
            respostas,
            resposta => Assert.Equal(HttpStatusCode.OK, resposta.StatusCode));

        var avaliacao = await cliente.GetFromJsonAsync<AvaliacaoResposta>(
            $"/avaliacoes/{jogoId}");

        Assert.NotNull(avaliacao);
        Assert.Contains(avaliacao!.Nota, new[] { 2, 4 });
        Assert.Equal(1, await ContarAvaliacoesAsync(jogoId));

        foreach (var resposta in respostas)
        {
            resposta.Dispose();
        }
    }

    private async Task<string> CriarContaAsync(HttpClient cliente)
    {
        var email = $"avaliacoes.{Guid.NewGuid():N}@teste.com";

        using var resposta = await cliente.PostAsJsonAsync(
            "/contas",
            new { email, senha = SenhaValida });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var autenticacao = await resposta.Content
            .ReadFromJsonAsync<RespostaAutenticacao>();

        Assert.NotNull(autenticacao);
        Assert.False(string.IsNullOrWhiteSpace(autenticacao!.Token));

        return autenticacao.Token;
    }

    private static Task AutenticarAsync(HttpClient cliente, string token)
    {
        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Semeia um jogo do catálogo com IdExterno único (limite de 50 caracteres)
    /// e devolve o id interno usado nas operações de avaliação.
    /// </summary>
    private async Task<int> CriarJogoAsync(string prefixo)
    {
        var opcoes = new DbContextOptionsBuilder<GameMatchDbContext>()
            .UseNpgsql(_fabrica.StringConexao)
            .Options;

        await using var contexto = new GameMatchDbContext(opcoes);

        var jogo = new Jogo
        {
            IdExterno = $"{prefixo[..Math.Min(prefixo.Length, 8)]}-{Guid.NewGuid():N}",
            Titulo = $"Jogo de teste {prefixo}",
            Descricao = "Jogo usado apenas pelos testes de avaliações.",
            Thumbnail = $"https://cdn.example/{prefixo}.jpg",
            Desenvolvedor = "Estúdio de teste",
            UrlFonte = $"https://www.freetogame.com/{prefixo}",
            SincronizadoEm = DateTimeOffset.UtcNow,
            Caracteristicas =
            [
                new JogoCaracteristica
                {
                    Tipo = TipoCaracteristica.Genero,
                    Valor = "RPG"
                },
                new JogoCaracteristica
                {
                    Tipo = TipoCaracteristica.Plataforma,
                    Valor = "PC (Windows)"
                }
            ]
        };

        contexto.Jogos.Add(jogo);
        await contexto.SaveChangesAsync();

        return jogo.Id;
    }

    private async Task<int> ContarAvaliacoesAsync(int jogoId)
    {
        var opcoes = new DbContextOptionsBuilder<GameMatchDbContext>()
            .UseNpgsql(_fabrica.StringConexao)
            .Options;

        await using var contexto = new GameMatchDbContext(opcoes);

        return await contexto.Avaliacoes.CountAsync(
            avaliacao => avaliacao.JogoId == jogoId);
    }

    private sealed record AvaliacaoResposta(int Nota);

    private sealed record RespostaAutenticacao(
        string Token,
        DateTimeOffset ExpiraEm,
        UsuarioResposta Usuario);

    private sealed record UsuarioResposta(string Email, DateTimeOffset CriadoEm);
}
