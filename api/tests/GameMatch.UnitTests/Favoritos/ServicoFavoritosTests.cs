using GameMatch.Application.Favoritos;
using GameMatch.Domain.Entidades;

namespace GameMatch.UnitTests.Favoritos;

public sealed class ServicoFavoritosTests
{
    [Fact]
    public async Task Adicionar_jogo_inexistente_devolve_nao_encontrado_sem_gravar()
    {
        var repositorio = new RepositorioFake { JogoExiste = false };
        var servico = new ServicoFavoritos(repositorio);

        var resultado = await servico.AdicionarAsync("usuario-1", 99);

        Assert.Equal(ResultadoFavoritar.JogoNaoEncontrado, resultado);
        Assert.Equal(0, repositorio.ChamadasAdicionar);
    }

    [Fact]
    public async Task Adicionar_jogo_novo_cria_o_favorito()
    {
        var repositorio = new RepositorioFake { JogoExiste = true };
        var servico = new ServicoFavoritos(repositorio);

        var resultado = await servico.AdicionarAsync("usuario-1", 7);

        Assert.Equal(ResultadoFavoritar.Criado, resultado);
        Assert.Equal(1, repositorio.ChamadasAdicionar);
    }

    [Fact]
    public async Task Adicionar_jogo_ja_favoritado_e_idempotente()
    {
        var repositorio = new RepositorioFake
        {
            JogoExiste = true,
            CriarAoAdicionar = false
        };
        var servico = new ServicoFavoritos(repositorio);

        var resultado = await servico.AdicionarAsync("usuario-1", 7);

        Assert.Equal(ResultadoFavoritar.JaExistente, resultado);
        Assert.Equal(1, repositorio.ChamadasAdicionar);
    }

    [Fact]
    public async Task Remover_favorito_existente_devolve_verdadeiro()
    {
        var repositorio = new RepositorioFake { Remover = true };
        var servico = new ServicoFavoritos(repositorio);

        Assert.True(await servico.RemoverAsync("usuario-1", 7));
    }

    [Fact]
    public async Task Remover_favorito_inexistente_devolve_falso()
    {
        var repositorio = new RepositorioFake { Remover = false };
        var servico = new ServicoFavoritos(repositorio);

        Assert.False(await servico.RemoverAsync("usuario-1", 7));
    }

    [Fact]
    public async Task Listar_mapeia_os_jogos_e_devolve_a_pagina()
    {
        var repositorio = new RepositorioFake
        {
            Total = 3,
            Jogos =
            [
                Jogo(2, ["RPG"], ["PC (Windows)"]),
                Jogo(1, ["Ação"], ["Web Browser"])
            ]
        };
        var servico = new ServicoFavoritos(repositorio);

        var resultado = await servico.ListarAsync(
            "usuario-1",
            new ConsultaFavoritos(Pagina: 1, Tamanho: 2));

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Pagina);
        Assert.Equal(3, resultado.Pagina!.Total);
        Assert.Equal(1, resultado.Pagina.Pagina);
        Assert.Equal(2, resultado.Pagina.Tamanho);
        Assert.Equal([2, 1], resultado.Pagina.Items.Select(item => item.Id));
        Assert.Equal(["RPG"], resultado.Pagina.Items[0].Generos);
        Assert.Equal(["Web Browser"], resultado.Pagina.Items[1].Plataformas);
    }

    [Fact]
    public async Task Listar_com_paginacao_invalida_nao_consulta_o_repositorio()
    {
        var repositorio = new RepositorioFake();
        var servico = new ServicoFavoritos(repositorio);

        var resultado = await servico.ListarAsync(
            "usuario-1",
            new ConsultaFavoritos(Pagina: 0, Tamanho: 0));

        Assert.False(resultado.Sucesso);
        Assert.Contains("pagina", resultado.Erros.Keys);
        Assert.Contains("tamanho", resultado.Erros.Keys);
        Assert.Equal(0, repositorio.ChamadasListar);
    }

    [Fact]
    public async Task Obter_ids_devolve_o_conjunto_do_usuario()
    {
        var repositorio = new RepositorioFake { Ids = [7, 9] };
        var servico = new ServicoFavoritos(repositorio);

        var ids = await servico.ObterIdsAsync("usuario-1");

        Assert.Equal([7, 9], ids.Ids);
    }

    private static Jogo Jogo(int id, string[] generos, string[] plataformas)
    {
        var jogo = new Jogo
        {
            Id = id,
            Titulo = $"Jogo {id}",
            Thumbnail = $"https://cdn.example/{id}.jpg",
            Desenvolvedor = "Estúdio"
        };

        jogo.Caracteristicas.Add(new JogoCaracteristica
        {
            JogoId = id,
            Tipo = TipoCaracteristica.Genero,
            Valor = generos[0]
        });

        jogo.Caracteristicas.Add(new JogoCaracteristica
        {
            JogoId = id,
            Tipo = TipoCaracteristica.Plataforma,
            Valor = plataformas[0]
        });

        return jogo;
    }

    private sealed class RepositorioFake : IFavoritoRepositorio
    {
        public bool JogoExiste { get; set; }

        public bool CriarAoAdicionar { get; set; } = true;

        public bool Remover { get; set; }

        public int Total { get; set; }

        public List<Jogo> Jogos { get; set; } = [];

        public List<int> Ids { get; set; } = [];

        public int ChamadasAdicionar { get; private set; }

        public int ChamadasListar { get; private set; }

        public Task<bool> JogoExisteAsync(
            int jogoId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(JogoExiste);

        public Task<bool> AdicionarAsync(
            string usuarioId,
            int jogoId,
            CancellationToken cancellationToken = default)
        {
            ChamadasAdicionar++;

            return Task.FromResult(CriarAoAdicionar);
        }

        public Task<bool> RemoverAsync(
            string usuarioId,
            int jogoId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Remover);

        public Task<PaginaFavoritos> ListarAsync(
            string usuarioId,
            ConsultaFavoritos consulta,
            CancellationToken cancellationToken = default)
        {
            ChamadasListar++;

            return Task.FromResult(new PaginaFavoritos(Jogos, Total));
        }

        public Task<IReadOnlyList<int>> ObterIdsAsync(
            string usuarioId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<int>>(Ids);
    }
}
