using GameMatch.Domain.Entidades;

namespace GameMatch.UnitTests.Entidades;

public sealed class PerfilGamerTests
{
    [Fact]
    public void Perfil_sem_preferencias_esta_incompleto()
    {
        var perfil = new PerfilGamer();

        Assert.False(perfil.EstaCompleto);
    }

    [Fact]
    public void Perfil_com_apenas_genero_esta_incompleto()
    {
        var perfil = CriarPerfil(TipoPreferencia.Genero, "RPG");

        Assert.False(perfil.EstaCompleto);
    }

    [Fact]
    public void Perfil_com_apenas_plataforma_esta_incompleto()
    {
        var perfil = CriarPerfil(TipoPreferencia.Plataforma, "PC (Windows)");

        Assert.False(perfil.EstaCompleto);
    }

    [Fact]
    public void Perfil_com_apenas_opcionais_esta_incompleto()
    {
        var perfil = CriarPerfil(
            TipoPreferencia.Estilo,
            "turn-based",
            TipoPreferencia.Experiencia,
            "exploração");

        Assert.False(perfil.EstaCompleto);
    }

    [Fact]
    public void Perfil_com_genero_e_plataforma_esta_completo()
    {
        var perfil = CriarPerfil(
            TipoPreferencia.Genero,
            "RPG",
            TipoPreferencia.Plataforma,
            "PC (Windows)");

        Assert.True(perfil.EstaCompleto);
    }

    [Fact]
    public void Opcionais_nao_alteram_a_completude()
    {
        var perfil = CriarPerfil(
            TipoPreferencia.Genero,
            "RPG",
            TipoPreferencia.Plataforma,
            "PC (Windows)",
            TipoPreferencia.Estilo,
            "turn-based",
            TipoPreferencia.Experiencia,
            "exploração");

        Assert.True(perfil.EstaCompleto);
    }

    private static PerfilGamer CriarPerfil(params object[] preferencias)
    {
        var perfil = new PerfilGamer { UsuarioId = "usuario-teste" };

        for (var indice = 0; indice < preferencias.Length; indice += 2)
        {
            perfil.Preferencias.Add(new PreferenciaPerfil
            {
                UsuarioId = perfil.UsuarioId,
                Tipo = (TipoPreferencia)preferencias[indice],
                Valor = (string)preferencias[indice + 1]
            });
        }

        return perfil;
    }
}
