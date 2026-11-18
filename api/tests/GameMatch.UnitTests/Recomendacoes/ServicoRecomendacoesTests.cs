using GameMatch.Application.Recomendacoes;
using GameMatch.Domain.Entidades;

namespace GameMatch.UnitTests.Recomendacoes;

public sealed class ServicoRecomendacoesTests
{
    [Fact]
    public async Task Executar_sem_perfil_devolve_perfil_incompleto_sem_ler_catalogo()
    {
        var repositorio = new RepositorioFake { Perfil = null };
        var servico = CriarServico(repositorio);

        var resultado = await servico.ExecutarAsync("usuario-1", experiencia: null);

        Assert.False(resultado.Sucesso);
        Assert.Null(resultado.Itens);
        Assert.Equal(
            ResultadoRecomendacoes.ErroPerfilIncompleto,
            resultado.CodigoErro);
        Assert.Contains("perfil", resultado.Erros.Keys);
        Assert.Equal(0, repositorio.ChamadasCatalogo);
    }

    [Fact]
    public async Task Executar_com_perfil_pela_metade_devolve_perfil_incompleto_sem_ler_catalogo()
    {
        var repositorio = new RepositorioFake
        {
            // Gênero sem plataforma: perfil incompleto (INV-02).
            Perfil = new PerfilGamer
            {
                UsuarioId = "usuario-1",
                Preferencias =
                [
                    new PreferenciaPerfil
                    {
                        UsuarioId = "usuario-1",
                        Tipo = TipoPreferencia.Genero,
                        Valor = "RPG"
                    }
                ]
            }
        };

        repositorio.Catalogo.Add(JogoPerfeito(1));
        var servico = CriarServico(repositorio);

        var resultado = await servico.ExecutarAsync("usuario-1", "explorar");

        Assert.False(resultado.Sucesso);
        Assert.Equal(
            ResultadoRecomendacoes.ErroPerfilIncompleto,
            resultado.CodigoErro);
        Assert.Equal(0, repositorio.ChamadasCatalogo);
    }

    [Fact]
    public async Task Executar_com_experiencia_desconhecida_devolve_404_sem_ler_catalogo()
    {
        var repositorio = new RepositorioFake { Perfil = PerfilCompleto() };
        repositorio.Catalogo.Add(JogoPerfeito(1));
        var servico = CriarServico(repositorio);

        var resultado = await servico.ExecutarAsync("usuario-1", "duelar");

        Assert.False(resultado.Sucesso);
        Assert.Equal(
            ResultadoRecomendacoes.ErroExperienciaDesconhecida,
            resultado.CodigoErro);
        Assert.Contains("experiencia", resultado.Erros.Keys);
        Assert.Equal(0, repositorio.ChamadasCatalogo);
    }

    [Fact]
    public async Task Executar_sem_experiencia_omite_o_fator_de_experiencia()
    {
        var repositorio = new RepositorioFake { Perfil = PerfilCompleto() };
        repositorio.Catalogo.Add(JogoPerfeito(1));
        var servico = CriarServico(repositorio);

        var resultado = await servico.ExecutarAsync("usuario-1", experiencia: null);

        Assert.True(resultado.Sucesso);
        var recomendacao = Assert.Single(resultado.Itens!);
        Assert.Equal(
            ["generos", "plataformas", "estilos"],
            recomendacao.Fatores.Select(fator => fator.Chave));
    }

    [Fact]
    public async Task Executar_com_experiencia_em_branco_trata_como_ausente()
    {
        var repositorio = new RepositorioFake { Perfil = PerfilCompleto() };
        repositorio.Catalogo.Add(JogoPerfeito(1));
        var servico = CriarServico(repositorio);

        var resultado = await servico.ExecutarAsync("usuario-1", "   ");

        Assert.True(resultado.Sucesso);
        var recomendacao = Assert.Single(resultado.Itens!);
        Assert.DoesNotContain(
            recomendacao.Fatores,
            fator => fator.Chave == "experiencia");
    }

    [Fact]
    public async Task Executar_com_experiencia_valida_inclui_o_fator_de_experiencia()
    {
        var repositorio = new RepositorioFake { Perfil = PerfilCompleto() };
        repositorio.Catalogo.Add(JogoPerfeito(1));
        var servico = CriarServico(repositorio, opcoes: [OpcaoExplorar()]);

        var resultado = await servico.ExecutarAsync("usuario-1", "explorar");

        Assert.True(resultado.Sucesso);
        Assert.Equal(1, repositorio.ChamadasCatalogo);

        var recomendacao = Assert.Single(resultado.Itens!);

        Assert.Equal(
            ["generos", "plataformas", "estilos", "experiencia"],
            recomendacao.Fatores.Select(fator => fator.Chave));

        // A soma dos fatores é exatamente a pontuação exibida.
        Assert.Equal(
            recomendacao.Pontuacao,
            recomendacao.Fatores.Sum(fator => fator.Contribuicao));
    }

    [Fact]
    public async Task Executar_ordena_por_pontuacao_decrescente_com_desempate_por_id_e_aplica_o_topo()
    {
        var repositorio = new RepositorioFake { Perfil = PerfilCompleto() };
        repositorio.Catalogo.AddRange(
            JogoNenhum(4),
            JogoSoGenero(3),
            JogoPerfeito(1),
            JogoSoGenero(2),
            JogoNenhum(5));

        var servico = CriarServico(repositorio, top: 3);

        var resultado = await servico.ExecutarAsync("usuario-1", experiencia: null);

        Assert.True(resultado.Sucesso);

        // Ordem: 100 (id 1), depois 47 com empate resolvido pelo menor id.
        var itens = resultado.Itens!;
        Assert.Equal([1, 2, 3], itens.Select(item => item.Jogo.Id));
        Assert.Equal([100, 47, 47], itens.Select(item => item.Pontuacao));
    }

    [Fact]
    public async Task Executar_inclui_jogo_sem_correspondencia_com_zero()
    {
        var repositorio = new RepositorioFake { Perfil = PerfilCompleto() };
        repositorio.Catalogo.Add(JogoNenhum(9));
        var servico = CriarServico(repositorio);

        var resultado = await servico.ExecutarAsync("usuario-1", experiencia: null);

        Assert.True(resultado.Sucesso);
        var recomendacao = Assert.Single(resultado.Itens!);
        Assert.Equal(0, recomendacao.Pontuacao);
        Assert.All(recomendacao.Fatores, fator => Assert.Equal(0, fator.Contribuicao));
    }

    [Fact]
    public async Task Listar_opcoes_devolve_chave_e_rotulo_na_ordem_configurada()
    {
        var servico = CriarServico(
            new RepositorioFake { Perfil = PerfilCompleto() },
            opcoes:
            [
                new OpcaoDescoberta { Chave = "relaxar", Rotulo = "Relaxar" },
                OpcaoExplorar()
            ]);

        var opcoes = servico.ListarOpcoes();

        Assert.Equal(["relaxar", "explorar"], opcoes.Select(opcao => opcao.Chave));
        Assert.Equal(["Relaxar", "Explorar"], opcoes.Select(opcao => opcao.Rotulo));
    }

    private static ServicoRecomendacoes CriarServico(
        RepositorioFake repositorio,
        int top = 12,
        OpcaoDescoberta[]? opcoes = null) =>
        new(
            repositorio,
            new OpcoesRecomendacoes { Top = top },
            new OpcoesDescoberta { Opcoes = [.. opcoes ?? []] });

    private static PerfilGamer PerfilCompleto() => new()
    {
        UsuarioId = "usuario-1",
        Preferencias =
        [
            new PreferenciaPerfil
            {
                UsuarioId = "usuario-1",
                Tipo = TipoPreferencia.Genero,
                Valor = "RPG"
            },
            new PreferenciaPerfil
            {
                UsuarioId = "usuario-1",
                Tipo = TipoPreferencia.Plataforma,
                Valor = "PC (Windows)"
            },
            new PreferenciaPerfil
            {
                UsuarioId = "usuario-1",
                Tipo = TipoPreferencia.Estilo,
                Valor = "pvp"
            }
        ]
    };

    private static OpcaoDescoberta OpcaoExplorar() => new()
    {
        Chave = "explorar",
        Rotulo = "Explorar",
        Generos = ["RPG"],
        Tags = ["open-world"]
    };

    // Correspondência completa nos três fatores: 100 pontos.
    private static JogoRecomendacao JogoPerfeito(int id) =>
        Jogo(id, ["RPG"], ["PC (Windows)"], ["pvp"]);

    // Só o gênero corresponde: 47 pontos com o perfil completo.
    private static JogoRecomendacao JogoSoGenero(int id) =>
        Jogo(id, ["RPG"], ["Web Browser"], ["tower-defense"]);

    private static JogoRecomendacao JogoNenhum(int id) =>
        Jogo(id, ["MOBA"], ["Web Browser"], ["tower-defense"]);

    private static JogoRecomendacao Jogo(
        int id,
        string[] generos,
        string[] plataformas,
        string[] tags) =>
        new(
            id,
            $"Jogo {id}",
            $"Descrição do jogo {id}.",
            $"https://cdn.example/{id}.jpg",
            "Estúdio",
            generos,
            plataformas,
            tags);

    private sealed class RepositorioFake : IRecomendacaoRepositorio
    {
        public PerfilGamer? Perfil { get; set; }

        public List<JogoRecomendacao> Catalogo { get; } = [];

        public int ChamadasCatalogo { get; private set; }

        public Task<PerfilGamer?> ObterPreferenciasAsync(
            string usuarioId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Perfil);

        public Task<IReadOnlyList<JogoRecomendacao>> ObterCatalogoAsync(
            CancellationToken cancellationToken = default)
        {
            ChamadasCatalogo++;

            return Task.FromResult<IReadOnlyList<JogoRecomendacao>>(Catalogo);
        }
    }
}
