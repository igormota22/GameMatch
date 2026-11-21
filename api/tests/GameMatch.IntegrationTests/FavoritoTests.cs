using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameMatch.Domain.Entidades;
using GameMatch.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.IntegrationTests;

/// <summary>
/// Cobertura de TC-07 para favoritos: 401, 201/200 idempotente, 204/404,
/// listagem paginada, ids marcados, INV-05 com POSTs paralelos e INV-07 entre
/// usuários.
/// </summary>
public sealed class FavoritoTests : IClassFixture<FabricaApiComBanco>
{
    private const string SenhaValida = "Senha123";

    private readonly FabricaApiComBanco _fabrica;

    public FavoritoTests(FabricaApiComBanco fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Favoritos_sem_token_retornam_401()
    {
        using var cliente = _fabrica.CreateClient();

        using var lista = await cliente.GetAsync("/favoritos");
        using var ids = await cliente.GetAsync("/favoritos/ids");
        using var criacao = await cliente.PostAsync("/favoritos/1", null);
        using var remocao = await cliente.DeleteAsync("/favoritos/1");

        Assert.Equal(HttpStatusCode.Unauthorized, lista.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, ids.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, criacao.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, remocao.StatusCode);
    }

    [Fact]
    public async Task Favoritar_cria_uma_vez_e_repeticao_e_idempotente()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var jogoId = await CriarJogoAsync("favorito-idempotente");

        using var criacao = await cliente.PostAsync($"/favoritos/{jogoId}", null);

        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);

        using var repeticao = await cliente.PostAsync($"/favoritos/{jogoId}", null);

        Assert.Equal(HttpStatusCode.OK, repeticao.StatusCode);

        var ids = await cliente.GetFromJsonAsync<FavoritosIds>("/favoritos/ids");

        Assert.NotNull(ids);
        Assert.Contains(jogoId, ids!.Ids);
        Assert.Equal(1, await ContarFavoritosAsync(jogoId));
    }

    [Fact]
    public async Task Favoritar_jogo_inexistente_retorna_404()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        using var resposta = await cliente.PostAsync("/favoritos/999999", null);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            resposta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Listar_favoritos_traz_os_jogos_com_paginacao()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var primeiro = await CriarJogoAsync("favorito-lista-1");
        var segundo = await CriarJogoAsync("favorito-lista-2");

        await cliente.PostAsync($"/favoritos/{primeiro}", null);
        await cliente.PostAsync($"/favoritos/{segundo}", null);

        var pagina1 = await cliente.GetFromJsonAsync<PaginaFavoritos>("/favoritos?tamanho=1&pagina=1");
        var pagina2 = await cliente.GetFromJsonAsync<PaginaFavoritos>("/favoritos?tamanho=1&pagina=2");

        Assert.NotNull(pagina1);
        Assert.NotNull(pagina2);
        Assert.Equal(2, pagina1!.Total);
        Assert.Single(pagina1.Items);
        Assert.Single(pagina2!.Items);
        Assert.Equal(
            new[] { primeiro, segundo }.OrderBy(id => id),
            new[] { pagina1.Items[0].Id, pagina2.Items[0].Id }.OrderBy(id => id));
        Assert.False(string.IsNullOrWhiteSpace(pagina1.Items[0].Titulo));
        Assert.NotEmpty(pagina1.Items[0].Generos);
    }

    [Fact]
    public async Task Remover_favorito_e_idempotente_na_ausencia()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var jogoId = await CriarJogoAsync("favorito-remocao");
        await cliente.PostAsync($"/favoritos/{jogoId}", null);

        using var remocao = await cliente.DeleteAsync($"/favoritos/{jogoId}");

        Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);

        using var repeticao = await cliente.DeleteAsync($"/favoritos/{jogoId}");

        Assert.Equal(HttpStatusCode.NotFound, repeticao.StatusCode);

        var lista = await cliente.GetFromJsonAsync<PaginaFavoritos>("/favoritos");

        Assert.NotNull(lista);
        Assert.Equal(0, lista!.Total);
    }

    [Fact]
    public async Task Favoritos_de_usuarios_diferentes_ficam_isolados()
    {
        using var clienteA = _fabrica.CreateClient();
        using var clienteB = _fabrica.CreateClient();

        await AutenticarAsync(clienteA, await CriarContaAsync(clienteA));
        await AutenticarAsync(clienteB, await CriarContaAsync(clienteB));

        var jogoId = await CriarJogoAsync("favorito-isolamento");

        await clienteA.PostAsync($"/favoritos/{jogoId}", null);

        var listaB = await clienteB.GetFromJsonAsync<PaginaFavoritos>("/favoritos");
        var idsB = await clienteB.GetFromJsonAsync<FavoritosIds>("/favoritos/ids");

        Assert.NotNull(listaB);
        Assert.NotNull(idsB);
        Assert.Equal(0, listaB!.Total);
        Assert.DoesNotContain(jogoId, idsB!.Ids);

        // B não remove o favorito de A (INV-07).
        using var remocaoB = await clienteB.DeleteAsync($"/favoritos/{jogoId}");

        Assert.Equal(HttpStatusCode.NotFound, remocaoB.StatusCode);
        Assert.Equal(1, await ContarFavoritosAsync(jogoId));
    }

    [Fact]
    public async Task Favoritar_em_paralelo_nao_cria_duplicata()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var jogoId = await CriarJogoAsync("favorito-paralelo");

        var respostas = await Task.WhenAll(
            cliente.PostAsync($"/favoritos/{jogoId}", null),
            cliente.PostAsync($"/favoritos/{jogoId}", null));

        var status = respostas.Select(resposta => resposta.StatusCode).OrderBy(codigo => (int)codigo);

        Assert.Equal(
            [HttpStatusCode.OK, HttpStatusCode.Created],
            status);
        Assert.Equal(1, await ContarFavoritosAsync(jogoId));

        foreach (var resposta in respostas)
        {
            resposta.Dispose();
        }
    }

    [Fact]
    public async Task Paginacao_invalida_retorna_400()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        using var resposta = await cliente.GetAsync("/favoritos?pagina=0");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            resposta.Content.Headers.ContentType?.MediaType);
    }

    private async Task<string> CriarContaAsync(HttpClient cliente)
    {
        var email = $"favoritos.{Guid.NewGuid():N}@teste.com";

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
    /// Semeia um jogo do catálogo com IdExterno único e devolve o id interno,
    /// usado pelas operações de favorito.
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
            Descricao = "Jogo usado apenas pelos testes de favoritos.",
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

    private async Task<int> ContarFavoritosAsync(int jogoId)
    {
        var opcoes = new DbContextOptionsBuilder<GameMatchDbContext>()
            .UseNpgsql(_fabrica.StringConexao)
            .Options;

        await using var contexto = new GameMatchDbContext(opcoes);

        return await contexto.Favoritos.CountAsync(favorito => favorito.JogoId == jogoId);
    }

    private sealed record FavoritosIds(List<int> Ids);

    private sealed record PaginaFavoritos(
        List<JogoFavorito> Items,
        int Pagina,
        int Tamanho,
        int Total);

    private sealed record JogoFavorito(
        int Id,
        string Titulo,
        string Thumbnail,
        List<string> Generos,
        List<string> Plataformas,
        string Desenvolvedor);

    private sealed record RespostaAutenticacao(
        string Token,
        DateTimeOffset ExpiraEm,
        UsuarioResposta Usuario);

    private sealed record UsuarioResposta(string Email, DateTimeOffset CriadoEm);
}
