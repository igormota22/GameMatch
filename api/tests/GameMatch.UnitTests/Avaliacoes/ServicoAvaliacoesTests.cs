using GameMatch.Application.Avaliacoes;
using GameMatch.Domain.Entidades;

namespace GameMatch.UnitTests.Avaliacoes;

public sealed class ServicoAvaliacoesTests
{
    [Fact]
    public async Task Obter_sem_avaliacao_devolve_nulo()
    {
        var repositorio = new RepositorioFake();
        var servico = new ServicoAvaliacoes(repositorio);

        var avaliacao = await servico.ObterAsync("usuario-1", 7);

        Assert.Null(avaliacao);
    }

    [Fact]
    public async Task Obter_com_avaliacao_devolve_a_nota()
    {
        var repositorio = new RepositorioFake
        {
            Avaliacao = new Avaliacao { UsuarioId = "usuario-1", JogoId = 7, Nota = 4 }
        };
        var servico = new ServicoAvaliacoes(repositorio);

        var avaliacao = await servico.ObterAsync("usuario-1", 7);

        Assert.NotNull(avaliacao);
        Assert.Equal(4, avaliacao!.Nota);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    public async Task Salvar_com_nota_fora_da_faixa_rejeita_sem_gravar(int nota)
    {
        var repositorio = new RepositorioFake { JogoExiste = true };
        var servico = new ServicoAvaliacoes(repositorio);

        var resultado = await servico.SalvarAsync("usuario-1", 7, nota);

        Assert.False(resultado.Sucesso);
        Assert.Equal(StatusSalvarAvaliacao.NotaInvalida, resultado.Status);
        Assert.Equal(0, repositorio.ChamadasSalvar);
    }

    [Fact]
    public async Task Salvar_com_jogo_inexistente_devolve_nao_encontrado_sem_gravar()
    {
        var repositorio = new RepositorioFake { JogoExiste = false };
        var servico = new ServicoAvaliacoes(repositorio);

        var resultado = await servico.SalvarAsync("usuario-1", 99, 3);

        Assert.Equal(StatusSalvarAvaliacao.JogoNaoEncontrado, resultado.Status);
        Assert.Equal(0, repositorio.ChamadasSalvar);
    }

    [Fact]
    public async Task Salvar_com_nota_valida_cria_a_avaliacao()
    {
        var repositorio = new RepositorioFake { JogoExiste = true };
        var servico = new ServicoAvaliacoes(repositorio);

        var resultado = await servico.SalvarAsync("usuario-1", 7, 5);

        Assert.True(resultado.Sucesso);
        Assert.Equal(5, resultado.Avaliacao!.Nota);
        Assert.Equal(1, repositorio.ChamadasSalvar);
        Assert.Equal(5, repositorio.NotaSalva);
    }

    [Fact]
    public async Task Salvar_novamente_substitui_a_nota_sem_duplicar()
    {
        var repositorio = new RepositorioFake
        {
            JogoExiste = true,
            Avaliacao = new Avaliacao { UsuarioId = "usuario-1", JogoId = 7, Nota = 2 }
        };
        var servico = new ServicoAvaliacoes(repositorio);

        var resultado = await servico.SalvarAsync("usuario-1", 7, 4);

        Assert.True(resultado.Sucesso);
        Assert.Equal(4, resultado.Avaliacao!.Nota);
        Assert.Equal(4, repositorio.NotaSalva);
    }

    [Fact]
    public async Task Remover_avaliacao_existente_devolve_verdadeiro()
    {
        var repositorio = new RepositorioFake { Remover = true };
        var servico = new ServicoAvaliacoes(repositorio);

        Assert.True(await servico.RemoverAsync("usuario-1", 7));
    }

    [Fact]
    public async Task Remover_avaliacao_inexistente_devolve_falso()
    {
        var repositorio = new RepositorioFake { Remover = false };
        var servico = new ServicoAvaliacoes(repositorio);

        Assert.False(await servico.RemoverAsync("usuario-1", 7));
    }

    private sealed class RepositorioFake : IAvaliacaoRepositorio
    {
        public bool JogoExiste { get; set; }

        public bool Remover { get; set; }

        public Avaliacao? Avaliacao { get; set; }

        public int? NotaSalva { get; private set; }

        public int ChamadasSalvar { get; private set; }

        public Task<bool> JogoExisteAsync(
            int jogoId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(JogoExiste);

        public Task<Avaliacao?> ObterAsync(
            string usuarioId,
            int jogoId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Avaliacao);

        public Task SalvarAsync(
            string usuarioId,
            int jogoId,
            int nota,
            CancellationToken cancellationToken = default)
        {
            ChamadasSalvar++;
            NotaSalva = nota;

            return Task.CompletedTask;
        }

        public Task<bool> RemoverAsync(
            string usuarioId,
            int jogoId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Remover);
    }
}
