using GameMatch.Application.Catalogo;
using GameMatch.Domain.Entidades;

namespace GameMatch.UnitTests.Catalogo;

public sealed class ServicoCatalogoTests
{
    [Fact]
    public async Task Listar_consulta_invalida_nao_aciona_o_repositorio()
    {
        var repositorio = new RepositorioFake();
        var servico = new ServicoCatalogo(repositorio);

        var resultado = await servico.ListarAsync(
            new ConsultaCatalogo(Pagina: 0, Tamanho: 500));

        Assert.False(resultado.Sucesso);
        Assert.Null(resultado.Pagina);
        Assert.Contains("pagina", resultado.Erros.Keys);
        Assert.Contains("tamanho", resultado.Erros.Keys);
        Assert.Equal(0, repositorio.Consultas);
    }

    [Fact]
    public async Task Listar_projeta_jogos_e_preserva_a_paginacao()
    {
        var repositorio = new RepositorioFake
        {
            Jogos =
            [
                JogoFake.Criar(1, "Aurora Tactics", "Strategy", "PC (Windows)", "action"),
                JogoFake.Criar(2, "Blue Racer", "Racing", "Web Browser")
            ],
            Total = 57
        };

        var resultado = await new ServicoCatalogo(repositorio).ListarAsync(
            new ConsultaCatalogo(Pagina: 3, Tamanho: 24, Busca: "a"));

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Pagina);
        Assert.Equal(3, resultado.Pagina!.Pagina);
        Assert.Equal(24, resultado.Pagina.Tamanho);
        Assert.Equal(57, resultado.Pagina.Total);
        Assert.Equal(2, resultado.Pagina.Items.Count);

        var resumo = resultado.Pagina.Items[0];
        Assert.Equal(1, resumo.Id);
        Assert.Equal("Aurora Tactics", resumo.Titulo);
        Assert.Equal(["Strategy"], resumo.Generos);
        Assert.Equal(["PC (Windows)"], resumo.Plataformas);
        Assert.Equal("Estúdio 1", resumo.Desenvolvedor);
    }

    [Fact]
    public async Task Obter_detalhe_inclui_caracteristicas_ordenadas_e_url_da_fonte()
    {
        var repositorio = new RepositorioFake
        {
            Jogo = JogoFake.Criar(
                7,
                "Aurora Tactics",
                "Strategy",
                "PC (Windows)",
                "turn-based",
                "co-op")
        };

        var detalhe = await new ServicoCatalogo(repositorio).ObterAsync(7);

        Assert.NotNull(detalhe);
        Assert.Equal(7, detalhe!.Id);
        Assert.Equal("Strategy", Assert.Single(detalhe.Generos));
        Assert.Equal("PC (Windows)", Assert.Single(detalhe.Plataformas));
        Assert.Equal(["co-op", "turn-based"], detalhe.Tags);
        Assert.Equal("https://www.freetogame.com/7", detalhe.UrlFonte);
        Assert.Equal("Descrição do jogo 7.", detalhe.Descricao);
    }

    [Fact]
    public async Task Obter_jogo_inexistente_retorna_nulo()
    {
        var detalhe = await new ServicoCatalogo(new RepositorioFake()).ObterAsync(404);

        Assert.Null(detalhe);
    }

    [Fact]
    public async Task ObterFiltros_repassa_as_listas_do_repositorio()
    {
        var esperado = new FiltrosCatalogoDto(
            ["Racing", "Strategy"],
            ["PC (Windows)", "Web Browser"],
            ["action", "co-op"]);

        var filtros = await new ServicoCatalogo(
            new RepositorioFake { Filtros = esperado })
            .ObterFiltrosAsync();

        Assert.Same(esperado, filtros);
    }

    private sealed class RepositorioFake : ICatalogoRepositorio
    {
        public IReadOnlyList<Jogo> Jogos { get; init; } = [];

        public int Total { get; init; }

        public Jogo? Jogo { get; init; }

        public FiltrosCatalogoDto Filtros { get; init; } =
            new([], [], []);

        public int Consultas { get; private set; }

        public Task<PaginaCatalogo> ListarAsync(
            ConsultaCatalogo consulta,
            CancellationToken cancellationToken = default)
        {
            Consultas++;

            return Task.FromResult(new PaginaCatalogo(Jogos, Total));
        }

        public Task<FiltrosCatalogoDto> ObterFiltrosAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Filtros);

        public Task<Jogo?> ObterPorIdAsync(
            int id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Jogo);
    }

    private static class JogoFake
    {
        public static Jogo Criar(
            int id,
            string titulo,
            string genero,
            string plataforma,
            params string[] tags)
        {
            var jogo = new Jogo
            {
                Id = id,
                IdExterno = id.ToString(),
                Titulo = titulo,
                Descricao = $"Descrição do jogo {id}.",
                Thumbnail = $"https://cdn.example/{id}.jpg",
                Desenvolvedor = $"Estúdio {id}",
                UrlFonte = $"https://www.freetogame.com/{id}",
                SincronizadoEm = DateTimeOffset.UtcNow
            };

            jogo.Caracteristicas.Add(new JogoCaracteristica
            {
                JogoId = id,
                Tipo = TipoCaracteristica.Genero,
                Valor = genero
            });

            jogo.Caracteristicas.Add(new JogoCaracteristica
            {
                JogoId = id,
                Tipo = TipoCaracteristica.Plataforma,
                Valor = plataforma
            });

            foreach (var tag in tags)
            {
                jogo.Caracteristicas.Add(new JogoCaracteristica
                {
                    JogoId = id,
                    Tipo = TipoCaracteristica.Estilo,
                    Valor = tag
                });
            }

            return jogo;
        }
    }
}
