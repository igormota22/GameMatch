using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GameMatch.Domain.Entidades;
using GameMatch.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameMatch.IntegrationTests;

public sealed class PerfilTests : IClassFixture<FabricaApiComBanco>
{
    private const string SenhaValida = "Senha123";

    private readonly FabricaApiComBanco _fabrica;

    public PerfilTests(FabricaApiComBanco fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Perfil_sem_token_retorna_401()
    {
        using var cliente = _fabrica.CreateClient();

        using var resposta = await cliente.GetAsync("/perfil");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Perfil_sem_configuracao_retorna_listas_vazias_e_incompleto()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        using var resposta = await cliente.GetAsync("/perfil");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var perfil = await resposta.Content.ReadFromJsonAsync<PerfilResposta>();
        Assert.NotNull(perfil);
        Assert.Empty(perfil!.Generos);
        Assert.Empty(perfil.Plataformas);
        Assert.Empty(perfil.Estilos);
        Assert.False(perfil.Completo);
        Assert.Null(perfil.AtualizadoEm);
    }

    [Fact]
    public async Task Salvar_perfil_com_opcionais_e_consultar()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var entrada = new
        {
            generos = new[] { "RPG" },
            plataformas = new[] { "PC (Windows)" },
            estilos = new[] { "turn-based" }
        };

        using var salvamento = await cliente.PutAsJsonAsync("/perfil", entrada);

        Assert.Equal(HttpStatusCode.OK, salvamento.StatusCode);

        var salvo = await salvamento.Content.ReadFromJsonAsync<PerfilResposta>();
        Assert.NotNull(salvo);
        Assert.Equal(["RPG"], salvo!.Generos);
        Assert.Equal(["turn-based"], salvo.Estilos);
        Assert.True(salvo.Completo);
        Assert.NotNull(salvo.AtualizadoEm);

        using var leitura = await cliente.GetAsync("/perfil");
        var consultado = await leitura.Content.ReadFromJsonAsync<PerfilResposta>();

        Assert.Equal(HttpStatusCode.OK, leitura.StatusCode);
        Assert.NotNull(consultado);
        Assert.Equal(["RPG"], consultado!.Generos);
        Assert.Equal(["PC (Windows)"], consultado.Plataformas);
        Assert.Equal(["turn-based"], consultado.Estilos);
        Assert.True(consultado.Completo);
    }

    [Fact]
    public async Task Salvar_sem_genero_retorna_422_e_preserva_perfil_anterior()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        await SalvarPerfilValidoAsync(cliente);

        using var resposta = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = Array.Empty<string>(),
                plataformas = new[] { "PC (Windows)" },
                estilos = Array.Empty<string>()
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Contains("generos", await resposta.Content.ReadAsStringAsync());

        var perfil = await cliente.GetFromJsonAsync<PerfilResposta>("/perfil");
        Assert.NotNull(perfil);
        Assert.Equal(["RPG"], perfil!.Generos);
        Assert.True(perfil.Completo);
    }

    [Fact]
    public async Task Salvar_sem_plataforma_retorna_422()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        using var resposta = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { "RPG" },
                plataformas = Array.Empty<string>(),
                estilos = Array.Empty<string>()
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Contains("plataformas", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Salvar_substitui_as_preferencias_anteriores()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        await SalvarPerfilValidoAsync(cliente);

        using var resposta = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { "Estratégia" },
                plataformas = new[] { "Web Browser" },
                estilos = Array.Empty<string>()
            });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var perfil = await cliente.GetFromJsonAsync<PerfilResposta>("/perfil");
        Assert.NotNull(perfil);
        Assert.Equal(["Estratégia"], perfil!.Generos);
        Assert.DoesNotContain("RPG", perfil.Generos);
        Assert.Equal(["Web Browser"], perfil.Plataformas);
        Assert.Empty(perfil.Estilos);
        Assert.True(perfil.Completo);
    }

    [Fact]
    public async Task Salvar_normaliza_e_deduplica_os_valores()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        using var resposta = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { "  RPG ", "rpg", "Ação" },
                plataformas = new[] { "PC (Windows)", " pc (windows) " },
                estilos = new[] { "turn-based", "TURN-BASED" }
            });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var perfil = await resposta.Content.ReadFromJsonAsync<PerfilResposta>();
        Assert.NotNull(perfil);
        Assert.Equal(["Ação", "RPG"], perfil!.Generos);
        Assert.Equal(["PC (Windows)"], perfil.Plataformas);
        Assert.Equal(["turn-based"], perfil.Estilos);
    }

    [Fact]
    public async Task Salvar_com_valor_acima_de_100_caracteres_retorna_422()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        using var resposta = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { new string('a', 101) },
                plataformas = new[] { "PC (Windows)" },
                estilos = Array.Empty<string>()
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Contains("100", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Salvar_sem_opcionais_e_aceito()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        using var resposta = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { "RPG" },
                plataformas = new[] { "PC (Windows)" },
                estilos = Array.Empty<string>()
            });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var perfil = await resposta.Content.ReadFromJsonAsync<PerfilResposta>();
        Assert.NotNull(perfil);
        Assert.Empty(perfil!.Estilos);
        Assert.True(perfil.Completo);
    }

    [Fact]
    public async Task Perfis_de_usuarios_diferentes_ficam_isolados()
    {
        using var clienteA = _fabrica.CreateClient();
        using var clienteB = _fabrica.CreateClient();

        await AutenticarAsync(clienteA, await CriarContaAsync(clienteA));
        await AutenticarAsync(clienteB, await CriarContaAsync(clienteB));

        await clienteA.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { "RPG" },
                plataformas = new[] { "PC (Windows)" },
                estilos = Array.Empty<string>()
            });

        await clienteB.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { "Corrida" },
                plataformas = new[] { "Web Browser" },
                estilos = Array.Empty<string>()
            });

        var perfilA = await clienteA.GetFromJsonAsync<PerfilResposta>("/perfil");
        var perfilB = await clienteB.GetFromJsonAsync<PerfilResposta>("/perfil");

        Assert.NotNull(perfilA);
        Assert.NotNull(perfilB);
        Assert.Equal(["RPG"], perfilA!.Generos);
        Assert.Equal(["Corrida"], perfilB!.Generos);
    }

    [Fact]
    public async Task Salvar_valor_fora_do_catalogo_retorna_422_sem_gravar()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticarAsync(cliente, await CriarContaAsync(cliente));

        var generoInedito = $"Genero Inedito {Guid.NewGuid():N}";
        var tagInedita = $"tag-inedita-{Guid.NewGuid():N}";

        using var resposta = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { generoInedito },
                plataformas = new[] { "PC (Windows)" },
                estilos = new[] { tagInedita }
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        Assert.Equal(
            "application/problem+json",
            resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("generos", corpo);
        Assert.Contains("estilos", corpo);

        var perfil = await cliente.GetFromJsonAsync<PerfilResposta>("/perfil");
        Assert.NotNull(perfil);
        Assert.Empty(perfil!.Generos);
        Assert.Empty(perfil.Estilos);
        Assert.False(perfil.Completo);
    }

    [Fact]
    public async Task Perfil_preserva_e_permite_remover_valor_legado_fora_do_catalogo()
    {
        using var cliente = _fabrica.CreateClient();
        var email = $"perfil.{Guid.NewGuid():N}@teste.com";
        await AutenticarAsync(cliente, await CriarContaComEmailAsync(cliente, email));
        await InserirPerfilLegadoAsync(email, "Órfão");

        using var manutencao = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { "Órfão" },
                plataformas = new[] { "PC (Windows)" },
                estilos = Array.Empty<string>()
            });

        Assert.Equal(HttpStatusCode.OK, manutencao.StatusCode);

        var comLegado = await cliente.GetFromJsonAsync<PerfilResposta>("/perfil");
        Assert.NotNull(comLegado);
        Assert.Equal(["Órfão"], comLegado!.Generos);

        using var remocao = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { "RPG" },
                plataformas = new[] { "PC (Windows)" },
                estilos = Array.Empty<string>()
            });

        Assert.Equal(HttpStatusCode.OK, remocao.StatusCode);

        var semLegado = await cliente.GetFromJsonAsync<PerfilResposta>("/perfil");
        Assert.NotNull(semLegado);
        Assert.Equal(["RPG"], semLegado!.Generos);

        using var recriacao = await cliente.PutAsJsonAsync(
            "/perfil",
            new
            {
                generos = new[] { "Órfão" },
                plataformas = new[] { "PC (Windows)" },
                estilos = Array.Empty<string>()
            });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, recriacao.StatusCode);
        Assert.Contains("generos", await recriacao.Content.ReadAsStringAsync());
    }

    private async Task<string> CriarContaAsync(HttpClient cliente)
    {
        await SemearCatalogoAsync();

        var email = $"perfil.{Guid.NewGuid():N}@teste.com";

        return await CriarContaComEmailAsync(cliente, email);
    }

    private static async Task<string> CriarContaComEmailAsync(
        HttpClient cliente,
        string email)
    {
        using var resposta = await cliente.PostAsJsonAsync(
            "/contas",
            new { email, senha = SenhaValida });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var autenticacao = await resposta.Content.ReadFromJsonAsync<RespostaAutenticacao>();
        Assert.NotNull(autenticacao);
        Assert.False(string.IsNullOrWhiteSpace(autenticacao!.Token));

        return autenticacao.Token;
    }

    private async Task SemearCatalogoAsync()
    {
        var opcoes = new DbContextOptionsBuilder<GameMatchDbContext>()
            .UseNpgsql(_fabrica.StringConexao)
            .Options;

        await using var contexto = new GameMatchDbContext(opcoes);

        if (await contexto.Jogos.AnyAsync(jogo => jogo.IdExterno == "perfil-catalogo-teste"))
        {
            return;
        }

        var jogo = new Jogo
        {
            IdExterno = "perfil-catalogo-teste",
            Titulo = "Catálogo de teste do perfil",
            Descricao = "Jogo usado apenas para fornecer características de teste.",
            Thumbnail = "https://cdn.example/perfil-catalogo-teste.jpg",
            Desenvolvedor = "Estúdio de teste",
            UrlFonte = "https://www.freetogame.com/perfil-catalogo-teste",
            SincronizadoEm = DateTimeOffset.UtcNow
        };

        AdicionarCaracteristicas(jogo, TipoCaracteristica.Genero, "RPG", "Ação", "Estratégia", "Corrida");
        AdicionarCaracteristicas(jogo, TipoCaracteristica.Plataforma, "PC (Windows)", "Web Browser");
        AdicionarCaracteristicas(jogo, TipoCaracteristica.Estilo, "turn-based");

        contexto.Jogos.Add(jogo);
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

    private async Task InserirPerfilLegadoAsync(string email, string genero)
    {
        using var escopo = _fabrica.Services.CreateScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<Usuario>>();
        var usuario = await usuarios.FindByEmailAsync(email);

        Assert.NotNull(usuario);

        var opcoes = new DbContextOptionsBuilder<GameMatchDbContext>()
            .UseNpgsql(_fabrica.StringConexao)
            .Options;

        await using var contexto = new GameMatchDbContext(opcoes);
        contexto.PerfisGamer.Add(new PerfilGamer
        {
            UsuarioId = usuario!.Id,
            AtualizadoEm = DateTimeOffset.UtcNow,
            Preferencias =
            [
                new PreferenciaPerfil
                {
                    UsuarioId = usuario.Id,
                    Tipo = TipoPreferencia.Genero,
                    Valor = genero
                },
                new PreferenciaPerfil
                {
                    UsuarioId = usuario.Id,
                    Tipo = TipoPreferencia.Plataforma,
                    Valor = "PC (Windows)"
                }
            ]
        });

        await contexto.SaveChangesAsync();
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

    private sealed record PerfilResposta(
        List<string> Generos,
        List<string> Plataformas,
        List<string> Estilos,
        bool Completo,
        DateTimeOffset? AtualizadoEm);

    private sealed record RespostaAutenticacao(
        string Token,
        DateTimeOffset ExpiraEm,
        UsuarioResposta Usuario);

    private sealed record UsuarioResposta(string Email, DateTimeOffset CriadoEm);
}
