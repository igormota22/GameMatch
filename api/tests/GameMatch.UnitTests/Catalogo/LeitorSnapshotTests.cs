using GameMatch.Seed.Snapshot;

namespace GameMatch.UnitTests.Catalogo;

public sealed class LeitorSnapshotTests
{
    [Fact]
    public void Ler_snapshot_valido_mapeia_jogos_tags_e_metadados()
    {
        Assert.True(
            Directory.Exists(DiretorioSnapshot.CaminhoFixo),
            $"Fixture não encontrada em {DiretorioSnapshot.CaminhoFixo}.");

        var snapshot = new LeitorSnapshot().Ler(DiretorioSnapshot.CaminhoFixo);

        Assert.Equal(2, snapshot.Jogos.Count);
        Assert.Equal(2, snapshot.Metadados.TotalJogos);

        var aurora = Assert.Single(
            snapshot.Jogos,
            jogo => jogo.IdExterno == "9001");

        Assert.Equal("Aurora Tactics", aurora.Titulo);
        Assert.Equal("Strategy", aurora.Genero);
        Assert.Equal("PC (Windows)", aurora.Plataforma);
        Assert.Equal("Aurora Studio", aurora.Desenvolvedor);
        Assert.Equal(
            "https://www.freetogame.com/aurora-tactics",
            aurora.UrlFonte);

        Assert.Equal(["9001"], snapshot.Tags["action"]);
        Assert.Equal(["9001", "9002"], snapshot.Tags["co-op"]);
        Assert.Equal(
            new DateTimeOffset(2026, 11, 16, 12, 0, 0, TimeSpan.Zero),
            snapshot.Metadados.ColetadoEm);
    }

    [Fact]
    public void Ler_arquivo_ausente_indica_o_arquivo()
    {
        using var diretorio = new DiretorioSnapshot()
            .ComTags(DiretorioSnapshot.Tags())
            .ComMetadados(DiretorioSnapshot.Metadados());

        var excecao = Assert.Throws<FormatoSnapshotInvalidoException>(
            () => new LeitorSnapshot().Ler(diretorio.Caminho));

        Assert.Contains("jogos.json", excecao.Message);
    }

    [Fact]
    public void Ler_json_malformado_indica_o_arquivo()
    {
        using var diretorio = new DiretorioSnapshot()
            .ComJogos("{ isto não é json")
            .ComTags(DiretorioSnapshot.Tags())
            .ComMetadados(DiretorioSnapshot.Metadados());

        var excecao = Assert.Throws<FormatoSnapshotInvalidoException>(
            () => new LeitorSnapshot().Ler(diretorio.Caminho));

        Assert.Contains("jogos.json", excecao.Message);
    }

    [Fact]
    public void Ler_id_duplicado_rejeita_o_snapshot()
    {
        var jogos = """
        [
          { "id": 9001, "title": "Aurora", "genre": "Strategy", "platform": "PC (Windows)" },
          { "id": 9001, "title": "Aurora 2", "genre": "Strategy", "platform": "PC (Windows)" }
        ]
        """;

        using var diretorio = new DiretorioSnapshot().Completo(
            jogos,
            """{ "tags": {} }""",
            DiretorioSnapshot.Metadados(totalJogos: 2, totalTags: 0));

        var excecao = Assert.Throws<FormatoSnapshotInvalidoException>(
            () => new LeitorSnapshot().Ler(diretorio.Caminho));

        Assert.Contains("9001", excecao.Message);
    }

    [Fact]
    public void Ler_jogo_sem_genero_rejeita_o_snapshot()
    {
        var jogos = """
        [
          { "id": 9001, "title": "Aurora", "genre": " ", "platform": "PC (Windows)" }
        ]
        """;

        using var diretorio = new DiretorioSnapshot().Completo(
            jogos,
            """{ "tags": {} }""",
            DiretorioSnapshot.Metadados(totalTags: 0));

        var excecao = Assert.Throws<FormatoSnapshotInvalidoException>(
            () => new LeitorSnapshot().Ler(diretorio.Caminho));

        Assert.Contains("Aurora", excecao.Message);
    }

    [Fact]
    public void Ler_metadados_inconsistentes_rejeita_o_snapshot()
    {
        using var diretorio = new DiretorioSnapshot().Completo(
            metadados: DiretorioSnapshot.Metadados(totalJogos: 99));

        var excecao = Assert.Throws<FormatoSnapshotInvalidoException>(
            () => new LeitorSnapshot().Ler(diretorio.Caminho));

        Assert.Contains("metadados", excecao.Message);
    }

    [Fact]
    public void Ler_sem_data_de_coleta_rejeita_o_snapshot()
    {
        using var diretorio = new DiretorioSnapshot().Completo(
            metadados: DiretorioSnapshot.Metadados(coletadoEm: "0001-01-01T00:00:00+00:00"));

        var excecao = Assert.Throws<FormatoSnapshotInvalidoException>(
            () => new LeitorSnapshot().Ler(diretorio.Caminho));

        Assert.Contains("data de coleta", excecao.Message);
    }

    [Fact]
    public void Ler_versao_de_formato_desconhecida_rejeita_o_snapshot()
    {
        using var diretorio = new DiretorioSnapshot().Completo(
            metadados: DiretorioSnapshot.Metadados(versaoFormato: 7));

        var excecao = Assert.Throws<FormatoSnapshotInvalidoException>(
            () => new LeitorSnapshot().Ler(diretorio.Caminho));

        Assert.Contains("Versão de formato", excecao.Message);
    }

    [Fact]
    public void Ler_tag_orfa_preserva_a_referencia_para_o_aviso_da_sincronizacao()
    {
        using var diretorio = new DiretorioSnapshot().Completo(
            DiretorioSnapshot.Jogos(9001),
            """{ "tags": { "action": [9001, 4242] } }""",
            DiretorioSnapshot.Metadados());

        var snapshot = new LeitorSnapshot().Ler(diretorio.Caminho);

        Assert.Equal(["4242", "9001"], snapshot.Tags["action"]);
    }

    [Fact]
    public void Ler_tags_sem_metadados_correspondentes_rejeita_o_snapshot()
    {
        using var diretorio = new DiretorioSnapshot().Completo(
            tags: """{ "tags": { "action": [9001], "co-op": [9001] } }""");

        var excecao = Assert.Throws<FormatoSnapshotInvalidoException>(
            () => new LeitorSnapshot().Ler(diretorio.Caminho));

        Assert.Contains("metadados", excecao.Message);
    }
}
