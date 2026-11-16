using GameMatch.Domain.Entidades;
using GameMatch.Infrastructure.Persistencia;
using GameMatch.Seed.Snapshot;
using GameMatch.Seed.Sincronizacao;
using Microsoft.EntityFrameworkCore;

namespace GameMatch.IntegrationTests;

public sealed class SincronizacaoCatalogoTests : IClassFixture<FabricaApiComBanco>
{
    private readonly FabricaApiComBanco _fabrica;

    public SincronizacaoCatalogoTests(FabricaApiComBanco fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Sync_duas_vezes_nao_duplica_e_reporta_atualizacoes()
    {
        var ids = IdsUnicos(2);
        var orfao = 999_999_999;

        using var diretorio = new DiretorioSnapshot()
            .ComJogos(Jogos(ids))
            .ComTags($$"""
                {
                  "tags": {
                    "action": [{{ids[0]}}],
                    "co-op": [{{ids[0]}}, {{ids[1]}}],
                    "fantasma": [{{orfao}}]
                  }
                }
                """)
            .ComMetadados(Metadados(totalJogos: 2, totalTags: 3));

        var snapshot = new LeitorSnapshot().Ler(diretorio.Caminho);

        await using (var contexto = CriarContexto())
        {
            var primeiro = await new SincronizadorCatalogo(contexto)
                .SincronizarAsync(snapshot);

            Assert.Equal(2, primeiro.Inseridos);
            Assert.Equal(0, primeiro.Atualizados);
            Assert.Equal(1, primeiro.ReferenciasIgnoradas);
        }

        await using (var contexto = CriarContexto())
        {
            var segundo = await new SincronizadorCatalogo(contexto)
                .SincronizarAsync(snapshot);

            Assert.Equal(0, segundo.Inseridos);
            Assert.Equal(2, segundo.Atualizados);
            Assert.Equal(1, segundo.ReferenciasIgnoradas);
        }

        await using var verificacao = CriarContexto();

        var idsExternos = ids.Select(id => id.ToString()).ToArray();

        var jogos = await verificacao.Jogos
            .AsNoTracking()
            .Include(jogo => jogo.Caracteristicas)
            .Where(jogo => idsExternos.Contains(jogo.IdExterno))
            .ToArrayAsync();

        Assert.Equal(2, jogos.Length);

        var comAction = Assert.Single(
            jogos,
            jogo => jogo.IdExterno == ids[0].ToString());

        Assert.Equal(4, comAction.Caracteristicas.Count);

        var semAction = Assert.Single(
            jogos,
            jogo => jogo.IdExterno == ids[1].ToString());

        Assert.Equal(3, semAction.Caracteristicas.Count);

        Assert.All(
            jogos,
            jogo => Assert.Equal(
                snapshot.Metadados.ColetadoEm,
                jogo.SincronizadoEm));
    }

    [Fact]
    public async Task Sync_substitui_as_caracteristicas_do_jogo()
    {
        var id = IdsUnicos(1)[0];

        using var diretorio = new DiretorioSnapshot()
            .ComJogos(Jogos([id]))
            .ComTags($$"""{ "tags": { "action": [{{id}}] } }""")
            .ComMetadados(Metadados(totalJogos: 1, totalTags: 1));

        await SincronizarAsync(diretorio.Caminho);

        diretorio
            .ComTags($$"""{ "tags": { "co-op": [{{id}}] } }""")
            .ComMetadados(Metadados(totalJogos: 1, totalTags: 1));

        await SincronizarAsync(diretorio.Caminho);

        await using var contexto = CriarContexto();

        var jogo = await contexto.Jogos
            .AsNoTracking()
            .Include(jogo => jogo.Caracteristicas)
            .SingleAsync(jogo => jogo.IdExterno == id.ToString());

        Assert.DoesNotContain(
            jogo.Caracteristicas,
            caracteristica => caracteristica is
            {
                Tipo: TipoCaracteristica.Estilo,
                Valor: "action"
            });

        Assert.Contains(
            jogo.Caracteristicas,
            caracteristica => caracteristica is
            {
                Tipo: TipoCaracteristica.Estilo,
                Valor: "co-op"
            });
    }

    [Fact]
    public async Task Snapshot_invalido_aborta_sem_alterar_o_banco()
    {
        var id = IdsUnicos(1)[0];

        using var valido = new DiretorioSnapshot()
            .ComJogos(Jogos([id]))
            .ComTags("""{ "tags": {} }""")
            .ComMetadados(Metadados(totalJogos: 1, totalTags: 0));

        await SincronizarAsync(valido.Caminho);

        await using var contexto = CriarContexto();

        var jogosAntes = await contexto.Jogos.CountAsync();
        var caracteristicasAntes = await contexto.JogosCaracteristicas.CountAsync();

        using var invalido = new DiretorioSnapshot()
            .ComJogos("{ isto não é json")
            .ComTags("""{ "tags": {} }""")
            .ComMetadados(Metadados(totalJogos: 1, totalTags: 0));

        Assert.Throws<FormatoSnapshotInvalidoException>(
            () => new LeitorSnapshot().Ler(invalido.Caminho));

        Assert.Equal(jogosAntes, await contexto.Jogos.CountAsync());
        Assert.Equal(
            caracteristicasAntes,
            await contexto.JogosCaracteristicas.CountAsync());
    }

    private async Task SincronizarAsync(string diretorio)
    {
        var snapshot = new LeitorSnapshot().Ler(diretorio);

        await using var contexto = CriarContexto();
        await new SincronizadorCatalogo(contexto).SincronizarAsync(snapshot);
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

    private static string Jogos(IReadOnlyList<int> ids) =>
        "[" + string.Join(
            ",",
            ids.Select(id => $$"""
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
                """)) + "]";

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
