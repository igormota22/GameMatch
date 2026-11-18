using GameMatch.Domain.Entidades;
using GameMatch.Infrastructure.Persistencia;
using GameMatch.Infrastructure.Persistencia.Repositorios;
using GameMatch.Seed.Snapshot;
using GameMatch.Seed.Sincronizacao;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.IntegrationTests;

/// <summary>
/// Cobertura das leituras de recomendação em PostgreSQL: preferências do
/// usuário e catálogo agrupado por tipo de característica.
/// </summary>
public sealed class RecomendacaoRepositorioTests : IClassFixture<FabricaApiComBanco>
{
    private readonly FabricaApiComBanco _fabrica;

    public RecomendacaoRepositorioTests(FabricaApiComBanco fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Obter_preferencias_devolve_o_perfil_do_usuario_e_nulo_se_inexistente()
    {
        var usuarioId = Guid.NewGuid().ToString();
        var email = $"recomendacoes.{Guid.NewGuid():N}@teste.com";

        await using (var gravacao = CriarContexto())
        {
            // A FK de perfis_gamer aponta para a conta, então o usuário é
            // criado junto com o perfil.
            gravacao.Users.Add(new Usuario
            {
                Id = usuarioId,
                UserName = email,
                Email = email,
                NormalizedUserName = email.ToUpperInvariant(),
                NormalizedEmail = email.ToUpperInvariant()
            });

            gravacao.PerfisGamer.Add(new PerfilGamer
            {
                UsuarioId = usuarioId,
                Preferencias =
                [
                    new PreferenciaPerfil
                    {
                        UsuarioId = usuarioId,
                        Tipo = TipoPreferencia.Genero,
                        Valor = "RPG"
                    },
                    new PreferenciaPerfil
                    {
                        UsuarioId = usuarioId,
                        Tipo = TipoPreferencia.Plataforma,
                        Valor = "PC (Windows)"
                    },
                    new PreferenciaPerfil
                    {
                        UsuarioId = usuarioId,
                        Tipo = TipoPreferencia.Estilo,
                        Valor = "turn-based"
                    }
                ]
            });

            await gravacao.SaveChangesAsync();
        }

        await using var leitura = CriarContexto();
        var repositorio = new RecomendacaoRepositorio(leitura);

        var perfil = await repositorio.ObterPreferenciasAsync(usuarioId);

        Assert.NotNull(perfil);
        Assert.Contains(
            perfil.Preferencias,
            preferencia => preferencia.Tipo == TipoPreferencia.Genero &&
                preferencia.Valor == "RPG");
        Assert.Contains(
            perfil.Preferencias,
            preferencia => preferencia.Tipo == TipoPreferencia.Plataforma &&
                preferencia.Valor == "PC (Windows)");
        Assert.Contains(
            perfil.Preferencias,
            preferencia => preferencia.Tipo == TipoPreferencia.Estilo &&
                preferencia.Valor == "turn-based");

        Assert.Null(await repositorio.ObterPreferenciasAsync("usuario-inexistente"));
    }

    [Fact]
    public async Task Obter_catalogo_agrupa_as_caracteristicas_por_tipo()
    {
        var id = IdsUnicos(1)[0];

        using var diretorio = new DiretorioSnapshot()
            .ComJogos(Jogos(id))
            .ComTags($$"""
                {
                  "tags": {
                    "turn-based": [{{id}}],
                    "pvp": [{{id}}]
                  }
                }
                """)
            .ComMetadados(Metadados(totalJogos: 1, totalTags: 2));

        var snapshot = new LeitorSnapshot().Ler(diretorio.Caminho);

        await using (var gravacao = CriarContexto())
        {
            await new SincronizadorCatalogo(gravacao).SincronizarAsync(snapshot);
        }

        await using var leitura = CriarContexto();
        var repositorio = new RecomendacaoRepositorio(leitura);

        var catalogo = await repositorio.ObterCatalogoAsync();

        var jogo = Assert.Single(
            catalogo,
            item => item.Titulo == $"Jogo de teste {id}");

        // Cada tipo de característica vai para o seu grupo, sem vazamento.
        Assert.Equal(["Strategy"], jogo.Generos);
        Assert.Equal(["PC (Windows)"], jogo.Plataformas);
        Assert.Equal(2, jogo.Tags.Count);
        Assert.Contains("turn-based", jogo.Tags);
        Assert.Contains("pvp", jogo.Tags);
    }

    private GameMatchDbContext CriarContexto() => new(
        new DbContextOptionsBuilder<GameMatchDbContext>()
            .UseNpgsql(_fabrica.StringConexao)
            .Options);

    private static int[] IdsUnicos(int quantidade)
    {
        var inicio = Random.Shared.Next(1_000_000, 8_000_000);

        return Enumerable.Range(inicio, quantidade).ToArray();
    }

    private static string Jogos(int id) =>
        $$"""
            [
              {
                "id": {{id}},
                "title": "Jogo de teste {{id}}",
                "thumbnail": "https://cdn.example/{{id}}.jpg",
                "short_description": "Descrição do jogo {{id}}.",
                "game_url": "https://www.freetogame.com/g/{{id}}",
                "genre": "Strategy",
                "platform": "PC (Windows)",
                "publisher": "Estúdio",
                "developer": "Estúdio",
                "release_date": "2024-01-01",
                "freetogame_profile_url": "https://www.freetogame.com/{{id}}"
              }
            ]
            """;

    private static string Metadados(int totalJogos, int totalTags) => $$"""
        {
          "coletadoEm": "2026-11-16T12:00:00+00:00",
          "totalJogos": {{totalJogos}},
          "totalTags": {{totalTags}},
          "tagsIgnoradas": [],
          "origem": "https://www.freetogame.com/api/",
          "versaoFormato": 1
        }
        """;
}
