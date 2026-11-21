using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameMatch.Domain.Entidades;
using GameMatch.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.IntegrationTests;

/// <summary>
/// Cobertura de TC-05/TC-06: 401, 409 `perfil-incompleto`, 404 de opção
/// desconhecida, topo-12 ordenado com fatores explicados e o Modo Descoberta.
/// </summary>
public sealed class RecomendacaoTests : IClassFixture<FabricaApiComBanco>
{
    private const string SenhaValida = "Senha123";

    private readonly FabricaApiComBanco _fabrica;

    public RecomendacaoTests(FabricaApiComBanco fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Recomendacoes_sem_token_retornam_401()
    {
        using var cliente = _fabrica.CreateClient();

        using var lista = await cliente.GetAsync("/recomendacoes");
        using var opcoes = await cliente.GetAsync("/recomendacoes/opcoes");

        Assert.Equal(HttpStatusCode.Unauthorized, lista.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, opcoes.StatusCode);
    }

    [Fact]
    public async Task Recomendacoes_com_perfil_incompleto_retornam_409_perfil_incompleto()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        using var resposta = await cliente.GetAsync("/recomendacoes");

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadAsStringAsync();

        Assert.Contains("perfil-incompleto", corpo);
        Assert.DoesNotContain("itens", corpo);
    }

    [Fact]
    public async Task Recomendacoes_com_experiencia_desconhecida_retornam_404()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));
        await SalvarPerfilValidoAsync(cliente);

        using var resposta = await cliente.GetAsync(
            "/recomendacoes?experiencia=duelar");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            resposta.Content.Headers.ContentType?.MediaType);

        Assert.Contains(
            "experiencia-desconhecida",
            await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Recomendacoes_com_perfil_completo_listam_o_topo_ordenado_com_fatores()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));
        await SalvarPerfilValidoAsync(cliente);

        using var resposta = await cliente.GetAsync("/recomendacoes");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<RespostaRecomendacoes>();

        Assert.NotNull(corpo);
        var itens = corpo!.Itens;

        // O catálogo de teste tem mais de 12 jogos: a resposta é o topo (DT-14).
        Assert.Equal(12, itens.Count);

        for (var indice = 1; indice < itens.Count; indice++)
        {
            var anterior = itens[indice - 1];
            var atual = itens[indice];

            Assert.True(
                anterior.Pontuacao >= atual.Pontuacao,
                "a pontuação deve ser decrescente");

            if (anterior.Pontuacao == atual.Pontuacao)
            {
                Assert.True(
                    anterior.Jogo.Id < atual.Jogo.Id,
                    "empates devem ordenar pelo menor id");
            }
        }

        foreach (var item in itens)
        {
            Assert.InRange(item.Pontuacao, 0, 100);
            Assert.False(
                string.IsNullOrWhiteSpace(item.Descricao),
                "cada recomendação deve trazer a descrição do jogo");

            // Sem experiência informada, os três fatores na ordem do contrato.
            Assert.Equal(
                ["generos", "plataformas", "estilos"],
                item.Fatores.Select(fator => fator.Chave));

            var soma = item.Fatores.Sum(fator => fator.Contribuicao);

            Assert.True(
                Math.Abs(soma - item.Pontuacao) <= 1,
                $"a soma das contribuições ({soma}) deve corresponder à " +
                $"pontuação ({item.Pontuacao})");

            Assert.All(
                item.Fatores,
                fator =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(fator.Rotulo));
                    Assert.NotNull(fator.Correspondencias);
                });
        }
    }

    [Fact]
    public async Task Recomendacoes_com_experiencia_valida_incluem_o_fator_de_experiencia()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));
        await SalvarPerfilValidoAsync(cliente);

        using var resposta = await cliente.GetAsync(
            "/recomendacoes?experiencia=explorar");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<RespostaRecomendacoes>();

        Assert.NotNull(corpo);
        Assert.NotEmpty(corpo!.Itens);

        Assert.All(
            corpo.Itens,
            item =>
            {
                Assert.InRange(item.Pontuacao, 0, 100);
                Assert.Contains(
                    item.Fatores,
                    fator => fator.Chave == "experiencia");
            });
    }

    [Fact]
    public async Task Opcoes_do_modo_descoberta_voltam_na_ordem_configurada()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        using var resposta = await cliente.GetAsync("/recomendacoes/opcoes");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var opcoes = await resposta.Content.ReadFromJsonAsync<List<OpcaoTeste>>();

        Assert.NotNull(opcoes);
        Assert.Equal(
            ["relaxar", "competir", "explorar", "cooperar", "historia"],
            opcoes!.Select(opcao => opcao.Chave));
        Assert.All(
            opcoes,
            opcao => Assert.False(string.IsNullOrWhiteSpace(opcao.Rotulo)));
    }

    [Fact]
    public async Task Favoritos_e_avaliacoes_saem_das_recomendacoes_e_voltam_apos_remocao()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));
        await SalvarPerfilValidoAsync(cliente);

        var inicial = await cliente.GetFromJsonAsync<RespostaRecomendacoes>(
            "/recomendacoes");

        Assert.NotNull(inicial);
        Assert.Equal(12, inicial!.Itens.Count);

        var favoritado = inicial.Itens[0].Jogo.Id;
        var avaliado = inicial.Itens[1].Jogo.Id;

        using var favorito = await cliente.PostAsync(
            $"/favoritos/{favoritado}",
            null);

        Assert.Equal(HttpStatusCode.Created, favorito.StatusCode);

        using var avaliacao = await cliente.PutAsJsonAsync(
            $"/avaliacoes/{avaliado}",
            new { nota = 4 });

        Assert.Equal(HttpStatusCode.OK, avaliacao.StatusCode);

        var semMarcados = await cliente.GetFromJsonAsync<RespostaRecomendacoes>(
            "/recomendacoes");

        Assert.NotNull(semMarcados);
        Assert.DoesNotContain(
            semMarcados!.Itens,
            item => item.Jogo.Id == favoritado);
        Assert.DoesNotContain(
            semMarcados.Itens,
            item => item.Jogo.Id == avaliado);
        Assert.Equal(12, semMarcados.Itens.Count);

        using var remocaoFavorito = await cliente.DeleteAsync(
            $"/favoritos/{favoritado}");
        using var remocaoAvaliacao = await cliente.DeleteAsync(
            $"/avaliacoes/{avaliado}");

        Assert.Equal(HttpStatusCode.NoContent, remocaoFavorito.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, remocaoAvaliacao.StatusCode);

        var restaurado = await cliente.GetFromJsonAsync<RespostaRecomendacoes>(
            "/recomendacoes");

        Assert.NotNull(restaurado);
        Assert.Contains(restaurado!.Itens, item => item.Jogo.Id == favoritado);
        Assert.Contains(restaurado.Itens, item => item.Jogo.Id == avaliado);
    }

    private async Task<string> CriarContaAsync(HttpClient cliente)
    {
        await SemearCatalogoAsync();

        var email = $"recomendacoes.{Guid.NewGuid():N}@teste.com";

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

    private static async Task SalvarPerfilValidoAsync(HttpClient cliente)
    {
        using var resposta = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { "RPG" },
                plataformas = new[] { "PC (Windows)" },
                estilos = new[] { "turn-based" }
            });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    /// <summary>
    /// Semeia mais de 12 jogos — metade compatível com o perfil de teste e
    /// metade incompatível — para exercitar o topo e a ordenação.
    /// </summary>
    private async Task SemearCatalogoAsync()
    {
        var opcoes = new DbContextOptionsBuilder<GameMatchDbContext>()
            .UseNpgsql(_fabrica.StringConexao)
            .Options;

        await using var contexto = new GameMatchDbContext(opcoes);

        if (await contexto.Jogos.AnyAsync(
                jogo => jogo.IdExterno == "recomendacao-catalogo-1"))
        {
            return;
        }

        for (var indice = 1; indice <= 15; indice++)
        {
            var jogo = new Jogo
            {
                IdExterno = $"recomendacao-catalogo-{indice}",
                Titulo = $"Recomendação de teste {indice}",
                Descricao = "Jogo usado apenas para testar recomendações.",
                Thumbnail = $"https://cdn.example/recomendacao-catalogo-{indice}.jpg",
                Desenvolvedor = "Estúdio de teste",
                UrlFonte =
                    $"https://www.freetogame.com/recomendacao-catalogo-{indice}",
                SincronizadoEm = DateTimeOffset.UtcNow
            };

            if (indice % 2 == 1)
            {
                AdicionarCaracteristicas(jogo, TipoCaracteristica.Genero, "RPG");
                AdicionarCaracteristicas(
                    jogo,
                    TipoCaracteristica.Plataforma,
                    "PC (Windows)");
                AdicionarCaracteristicas(
                    jogo,
                    TipoCaracteristica.Estilo,
                    "turn-based");
            }
            else
            {
                AdicionarCaracteristicas(jogo, TipoCaracteristica.Genero, "MOBA");
                AdicionarCaracteristicas(
                    jogo,
                    TipoCaracteristica.Plataforma,
                    "Web Browser");
                AdicionarCaracteristicas(jogo, TipoCaracteristica.Estilo, "pvp");
            }

            contexto.Jogos.Add(jogo);
        }

        await contexto.SaveChangesAsync();
    }

    private static void AdicionarCaracteristicas(
        Jogo jogo,
        TipoCaracteristica tipo,
        params string[] valores)
    {
        foreach (var valor in valores)
        {
            jogo.Caracteristicas.Add(new JogoCaracteristica
            {
                Tipo = tipo,
                Valor = valor
            });
        }
    }

    private sealed record RespostaRecomendacoes(List<RecomendacaoTeste> Itens);

    private sealed record RecomendacaoTeste(
        JogoTeste Jogo,
        string Descricao,
        int Pontuacao,
        List<FatorTeste> Fatores);

    private sealed record JogoTeste(
        int Id,
        string Titulo,
        string Thumbnail,
        List<string> Generos,
        List<string> Plataformas,
        string Desenvolvedor);

    private sealed record FatorTeste(
        string Chave,
        string Rotulo,
        int Contribuicao,
        List<string> Correspondencias);

    private sealed record OpcaoTeste(string Chave, string Rotulo);

    private sealed record RespostaAutenticacao(
        string Token,
        DateTimeOffset ExpiraEm,
        UsuarioResposta Usuario);

    private sealed record UsuarioResposta(string Email, DateTimeOffset CriadoEm);
}
