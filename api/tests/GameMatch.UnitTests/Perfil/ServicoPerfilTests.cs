using GameMatch.Application.Perfil;
using GameMatch.Domain.Entidades;

namespace GameMatch.UnitTests.Perfil;

public sealed class ServicoPerfilTests
{
    [Fact]
    public async Task Obter_sem_perfil_retorna_estado_vazio_sem_gravar()
    {
        var repositorio = new RepositorioFake();
        var servico = new ServicoPerfil(repositorio);

        var perfil = await servico.ObterAsync("usuario-1");

        Assert.Empty(perfil.Generos);
        Assert.Empty(perfil.Plataformas);
        Assert.Empty(perfil.Estilos);
        Assert.Empty(perfil.Experiencias);
        Assert.False(perfil.Completo);
        Assert.Null(perfil.AtualizadoEm);
        Assert.Equal(0, repositorio.ChamadasSalvar);
    }

    [Fact]
    public async Task Salvar_cria_perfil_completo_e_normaliza_os_valores()
    {
        var repositorio = new RepositorioFake();
        var servico = new ServicoPerfil(repositorio);

        var resultado = await servico.SalvarAsync(
            "usuario-1",
            new PerfilSalvarDto(
                ["  RPG ", "rpg", "Ação"],
                ["PC (Windows)", " pc (windows) "],
                ["turn-based"],
                null));

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Perfil);
        Assert.Equal(["Ação", "RPG"], resultado.Perfil!.Generos);
        Assert.Equal(["PC (Windows)"], resultado.Perfil.Plataformas);
        Assert.Equal(["turn-based"], resultado.Perfil.Estilos);
        Assert.Empty(resultado.Perfil.Experiencias);
        Assert.True(resultado.Perfil.Completo);
        Assert.NotNull(resultado.Perfil.AtualizadoEm);
        Assert.Equal(1, repositorio.ChamadasSalvar);
    }

    [Fact]
    public async Task Salvar_sem_genero_retorna_erro_e_nao_grava()
    {
        var repositorio = new RepositorioFake();
        var servico = new ServicoPerfil(repositorio);

        var resultado = await servico.SalvarAsync(
            "usuario-1",
            new PerfilSalvarDto(null, ["PC (Windows)"], null, null));

        Assert.False(resultado.Sucesso);
        Assert.Contains("generos", resultado.Erros.Keys);
        Assert.Null(repositorio.Perfil);
        Assert.Equal(0, repositorio.ChamadasSalvar);
    }

    [Fact]
    public async Task Salvar_sem_plataforma_retorna_erro_e_nao_grava()
    {
        var repositorio = new RepositorioFake();
        var servico = new ServicoPerfil(repositorio);

        var resultado = await servico.SalvarAsync(
            "usuario-1",
            new PerfilSalvarDto(["RPG"], [], null, null));

        Assert.False(resultado.Sucesso);
        Assert.Contains("plataformas", resultado.Erros.Keys);
        Assert.Null(repositorio.Perfil);
    }

    [Fact]
    public async Task Salvar_com_valor_acima_do_limite_retorna_erro()
    {
        var repositorio = new RepositorioFake();
        var servico = new ServicoPerfil(repositorio);
        var valorLongo = new string('a', 101);

        var resultado = await servico.SalvarAsync(
            "usuario-1",
            new PerfilSalvarDto([valorLongo], ["PC (Windows)"], null, null));

        Assert.False(resultado.Sucesso);
        Assert.Contains("generos", resultado.Erros.Keys);
        Assert.Equal(0, repositorio.ChamadasSalvar);
    }

    [Fact]
    public async Task Salvar_substitui_as_preferencias_anteriores()
    {
        var repositorio = new RepositorioFake
        {
            Perfil = new PerfilGamer
            {
                UsuarioId = "usuario-1",
                AtualizadoEm = DateTimeOffset.UtcNow.AddDays(-1),
                Preferencias =
                [
                    new PreferenciaPerfil
                    {
                        UsuarioId = "usuario-1",
                        Tipo = TipoPreferencia.Genero,
                        Valor = "Antigo"
                    },
                    new PreferenciaPerfil
                    {
                        UsuarioId = "usuario-1",
                        Tipo = TipoPreferencia.Plataforma,
                        Valor = "Antiga"
                    }
                ]
            }
        };
        var servico = new ServicoPerfil(repositorio);
        var antes = DateTimeOffset.UtcNow;

        var resultado = await servico.SalvarAsync(
            "usuario-1",
            new PerfilSalvarDto(["Novo"], ["Nova"], null, null));

        Assert.True(resultado.Sucesso);
        Assert.NotNull(repositorio.Perfil);
        Assert.DoesNotContain(
            repositorio.Perfil!.Preferencias,
            preferencia => preferencia.Valor == "Antigo");
        Assert.Contains(
            repositorio.Perfil.Preferencias,
            preferencia => preferencia.Valor == "Novo");
        Assert.True(repositorio.Perfil.AtualizadoEm >= antes);
    }

    [Fact]
    public async Task Obter_perfil_com_opcionais_sem_obrigatorios_retorna_incompleto()
    {
        var repositorio = new RepositorioFake
        {
            Perfil = new PerfilGamer
            {
                UsuarioId = "usuario-1",
                Preferencias =
                [
                    new PreferenciaPerfil
                    {
                        UsuarioId = "usuario-1",
                        Tipo = TipoPreferencia.Estilo,
                        Valor = "turn-based"
                    }
                ]
            }
        };
        var servico = new ServicoPerfil(repositorio);

        var perfil = await servico.ObterAsync("usuario-1");

        Assert.False(perfil.Completo);
    }

    [Fact]
    public async Task Obter_perfil_com_genero_e_plataforma_retorna_completo()
    {
        var repositorio = new RepositorioFake
        {
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
                    },
                    new PreferenciaPerfil
                    {
                        UsuarioId = "usuario-1",
                        Tipo = TipoPreferencia.Plataforma,
                        Valor = "PC (Windows)"
                    }
                ]
            }
        };
        var servico = new ServicoPerfil(repositorio);

        var perfil = await servico.ObterAsync("usuario-1");

        Assert.True(perfil.Completo);
    }

    private sealed class RepositorioFake : IPerfilRepositorio
    {
        public PerfilGamer? Perfil { get; set; }

        public int ChamadasSalvar { get; private set; }

        public Task<PerfilGamer?> ObterAsync(
            string usuarioId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Perfil);

        public Task SubstituirAsync(
            PerfilGamer perfil,
            CancellationToken cancellationToken = default)
        {
            ChamadasSalvar++;
            Perfil = perfil;

            return Task.CompletedTask;
        }
    }
}
