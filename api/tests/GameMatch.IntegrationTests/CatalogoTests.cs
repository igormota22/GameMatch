using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameMatch.Domain.Entidades;
using GameMatch.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.IntegrationTests;

public sealed class CatalogoTests : IClassFixture<FabricaApiComBanco>
{
    private const string SenhaValida = "Senha123";

    private readonly FabricaApiComBanco _fabrica;

    public CatalogoTests(FabricaApiComBanco fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Catalogo_sem_token_retorna_401_em_todos_os_endpoints()
    {
        using var cliente = _fabrica.CreateClient();

        foreach (var rota in new[] { "/catalogo", "/catalogo/filtros", "/catalogo/1" })
        {
            using var resposta = await cliente.GetAsync(rota);

            Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
            Assert.Equal(
                "application/problem+json",
                resposta.Content.Headers.ContentType?.MediaType);
        }
    }

    [Theory]
    [InlineData("/catalogo?pagina=0", "pagina")]
    [InlineData("/catalogo?pagina=-2", "pagina")]
    [InlineData("/catalogo?tamanho=0", "tamanho")]
    [InlineData("/catalogo?tamanho=101", "tamanho")]
    public async Task Catalogo_paginacao_invalida_retorna_400(string rota, string campo)
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente);

        using var resposta = await cliente.GetAsync(rota);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            resposta.Content.Headers.ContentType?.MediaType);
        Assert.Contains(campo, await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Catalogo_pagina_em_ordem_estavel_e_com_total()
    {
        var prefixo = Prefixo();
        var ids = await SemearAsync(
            prefixo,
            Semente("Aurora", "Strategy", "PC (Windows)", "action"),
            Semente("Blue", "Racing", "Web Browser", "racing"),
            Semente("Cobalt", "Strategy", "PC (Windows)", "strategy"));

        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente);

        using var primeira = await cliente.GetAsync(
            $"/catalogo?busca={prefixo}&tamanho=2&pagina=1");
        var pagina1 = await primeira.Content.ReadFromJsonAsync<PaginaResposta>();

        Assert.Equal(HttpStatusCode.OK, primeira.StatusCode);
        Assert.NotNull(pagina1);
        Assert.Equal(3, pagina1!.Total);
        Assert.Equal(1, pagina1.Pagina);
        Assert.Equal(2, pagina1.Tamanho);
        Assert.Equal(
            [$"{prefixo} Aurora", $"{prefixo} Blue"],
            pagina1.Items.Select(item => item.Titulo));

        using var segunda = await cliente.GetAsync(
            $"/catalogo?busca={prefixo}&tamanho=2&pagina=2");
        var pagina2 = await segunda.Content.ReadFromJsonAsync<PaginaResposta>();

        Assert.NotNull(pagina2);
        Assert.Equal(3, pagina2!.Total);
        Assert.Single(pagina2.Items);
        Assert.Equal($"{prefixo} Cobalt", pagina2.Items[0].Titulo);

        using var terceira = await cliente.GetAsync(
            $"/catalogo?busca={prefixo}&tamanho=2&pagina=3");
        var pagina3 = await terceira.Content.ReadFromJsonAsync<PaginaResposta>();

        Assert.NotNull(pagina3);
        Assert.Equal(3, pagina3!.Total);
        Assert.Empty(pagina3.Items);

        var titulos = pagina1.Items
            .Concat(pagina2.Items)
            .Select(item => item.Titulo)
            .ToArray();

        Assert.Equal(3, titulos.Distinct().Count());
        Assert.All(ids, id => Assert.True(id > 0));
    }

    [Fact]
    public async Task Catalogo_sem_resultados_retorna_200_com_total_zero()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente);

        using var resposta = await cliente.GetAsync(
            $"/catalogo?busca=inexistente-{Guid.NewGuid():N}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var pagina = await resposta.Content.ReadFromJsonAsync<PaginaResposta>();

        Assert.NotNull(pagina);
        Assert.Equal(0, pagina!.Total);
        Assert.Empty(pagina.Items);
    }

    [Fact]
    public async Task Busca_ignora_caixa_e_trata_curingas_literalmente()
    {
        var prefixo = Prefixo();
        await SemearAsync(prefixo, Semente("Nebula", "Strategy", "PC (Windows)", "action"));

        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente);

        var termo = prefixo[6..].ToLowerInvariant();

        var parcial = await cliente.GetFromJsonAsync<PaginaResposta>(
            $"/catalogo?busca={termo}");
        Assert.NotNull(parcial);
        Assert.Equal(1, parcial!.Total);

        var caixaAlta = await cliente.GetFromJsonAsync<PaginaResposta>(
            $"/catalogo?busca={Uri.EscapeDataString($"{prefixo.ToUpperInvariant()} NEBULA")}");
        Assert.NotNull(caixaAlta);
        Assert.Equal(1, caixaAlta!.Total);

        var curinga = await cliente.GetFromJsonAsync<PaginaResposta>(
            $"/catalogo?busca={Uri.EscapeDataString($"{prefixo}%")}");
        Assert.NotNull(curinga);
        Assert.Equal(0, curinga!.Total);

        var sublinhado = await cliente.GetFromJsonAsync<PaginaResposta>(
            $"/catalogo?busca={Uri.EscapeDataString($"{prefixo}_")}");
        Assert.NotNull(sublinhado);
        Assert.Equal(0, sublinhado!.Total);
    }

    [Fact]
    public async Task Filtros_funcionam_isolados_e_combinados_com_a_busca()
    {
        var prefixo = Prefixo();
        await SemearAsync(
            prefixo,
            Semente("Aurora", "Strategy", "PC (Windows)", "action"),
            Semente("Blue", "Strategy", "Web Browser", "co-op"),
            Semente("Cobalt", "Racing", "PC (Windows)", "action"));

        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente);

        async Task<int> TotalAsync(params string[] filtros)
        {
            var query = $"/catalogo?busca={prefixo}" +
                string.Concat(filtros.Select(filtro => $"&{filtro}"));

            var pagina = await cliente.GetFromJsonAsync<PaginaResposta>(query);

            Assert.NotNull(pagina);
            return pagina!.Total;
        }

        Assert.Equal(2, await TotalAsync("genero=Strategy"));
        Assert.Equal(1, await TotalAsync("plataforma=Web Browser"));
        Assert.Equal(2, await TotalAsync("tag=action"));
        Assert.Equal(
            1,
            await TotalAsync(
                "genero=Strategy",
                $"plataforma={Uri.EscapeDataString("PC (Windows)")}",
                "tag=action"));
        Assert.Equal(0, await TotalAsync("genero=Inexistente"));
    }

    [Fact]
    public async Task Filtros_listam_valores_distintos_e_ordenados()
    {
        var prefixo = Prefixo();
        await SemearAsync(
            prefixo,
            Semente("Aurora", "Strategy", "PC (Windows)", "action"),
            Semente("Blue", "Strategy", "Web Browser", "co-op"),
            Semente("Cobalt", "Racing", "PC (Windows)", "action"));

        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente);

        using var resposta = await cliente.GetAsync("/catalogo/filtros");
        var filtros = await resposta.Content.ReadFromJsonAsync<FiltrosResposta>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(filtros);
        Assert.Contains("Strategy", filtros!.Generos);
        Assert.Contains("Racing", filtros.Generos);
        Assert.Contains("PC (Windows)", filtros.Plataformas);
        Assert.Contains("action", filtros.Tags);

        Assert.Equal(
            filtros.Generos.OrderBy(valor => valor, StringComparer.OrdinalIgnoreCase),
            filtros.Generos);
        Assert.Equal(
            filtros.Plataformas.OrderBy(valor => valor, StringComparer.OrdinalIgnoreCase),
            filtros.Plataformas);
        Assert.Equal(
            filtros.Tags.OrderBy(valor => valor, StringComparer.OrdinalIgnoreCase),
            filtros.Tags);
        Assert.Equal(filtros.Generos.Distinct().Count(), filtros.Generos.Count);
    }

    [Fact]
    public async Task Detalhe_retorna_dados_completos_do_jogo()
    {
        var prefixo = Prefixo();
        var ids = await SemearAsync(
            prefixo,
            Semente("Aurora", "Strategy", "PC (Windows)", "action", "co-op"));

        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente);

        using var resposta = await cliente.GetAsync($"/catalogo/{ids[0]}");
        var jogo = await resposta.Content.ReadFromJsonAsync<DetalheResposta>();

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(jogo);
        Assert.Equal($"{prefixo} Aurora", jogo!.Titulo);
        Assert.Equal("Descrição do jogo de teste.", jogo.Descricao);
        Assert.Equal("Estúdio de teste", jogo.Desenvolvedor);
        Assert.Equal("https://www.freetogame.com/aurora", jogo.UrlFonte);
        Assert.Equal(["Strategy"], jogo.Generos);
        Assert.Equal(["PC (Windows)"], jogo.Plataformas);
        Assert.Contains("action", jogo.Tags);
        Assert.Contains("co-op", jogo.Tags);
        Assert.False(string.IsNullOrWhiteSpace(jogo.Thumbnail));
    }

    [Theory]
    [InlineData("/catalogo/999999999")]
    [InlineData("/catalogo/abc")]
    public async Task Detalhe_inexistente_ou_invalido_retorna_404(string rota)
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente);

        using var resposta = await cliente.GetAsync(rota);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            resposta.Content.Headers.ContentType?.MediaType);
    }

    private static string Prefixo() => $"cat-{Guid.NewGuid():N}"[..16];

    private static JogoSemente Semente(
        string titulo,
        string genero,
        string plataforma,
        params string[] tags) => new(titulo, genero, plataforma, tags);

    private async Task<int[]> SemearAsync(
        string prefixo,
        params JogoSemente[] jogos)
    {
        var opcoes = new DbContextOptionsBuilder<GameMatchDbContext>()
            .UseNpgsql(_fabrica.StringConexao)
            .Options;

        await using var contexto = new GameMatchDbContext(opcoes);

        var entidades = jogos.Select((jogo, indice) =>
        {
            var entidade = new Jogo
            {
                IdExterno = $"teste-{prefixo}-{indice}",
                Titulo = $"{prefixo} {jogo.Titulo}",
                Descricao = "Descrição do jogo de teste.",
                Thumbnail = $"https://cdn.example/{prefixo}-{indice}.jpg",
                Desenvolvedor = "Estúdio de teste",
                UrlFonte = "https://www.freetogame.com/aurora",
                SincronizadoEm = DateTimeOffset.UtcNow
            };

            entidade.Caracteristicas.Add(new JogoCaracteristica
            {
                Tipo = TipoCaracteristica.Genero,
                Valor = jogo.Genero
            });

            entidade.Caracteristicas.Add(new JogoCaracteristica
            {
                Tipo = TipoCaracteristica.Plataforma,
                Valor = jogo.Plataforma
            });

            foreach (var tag in jogo.Tags)
            {
                entidade.Caracteristicas.Add(new JogoCaracteristica
                {
                    Tipo = TipoCaracteristica.Estilo,
                    Valor = tag
                });
            }

            return entidade;
        }).ToArray();

        contexto.Jogos.AddRange(entidades);
        await contexto.SaveChangesAsync();

        return entidades.Select(entidade => entidade.Id).ToArray();
    }

    private async Task AutenticarAsync(HttpClient cliente)
    {
        var email = $"catalogo.{Guid.NewGuid():N}@teste.com";

        using var resposta = await cliente.PostAsJsonAsync(
            "/contas",
            new { email, senha = SenhaValida });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var autenticacao = await resposta.Content.ReadFromJsonAsync<RespostaAutenticacao>();

        Assert.NotNull(autenticacao);
        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", autenticacao!.Token);
    }

    private sealed record JogoSemente(
        string Titulo,
        string Genero,
        string Plataforma,
        string[] Tags);

    private sealed record PaginaResposta(
        List<ItemResposta> Items,
        int Pagina,
        int Tamanho,
        int Total);

    private sealed record ItemResposta(
        int Id,
        string Titulo,
        string Thumbnail,
        List<string> Generos,
        List<string> Plataformas,
        string Desenvolvedor);

    private sealed record FiltrosResposta(
        List<string> Generos,
        List<string> Plataformas,
        List<string> Tags);

    private sealed record DetalheResposta(
        int Id,
        string Titulo,
        string Descricao,
        string Thumbnail,
        string Desenvolvedor,
        string UrlFonte,
        List<string> Generos,
        List<string> Plataformas,
        List<string> Tags);

    private sealed record RespostaAutenticacao(string Token);
}
