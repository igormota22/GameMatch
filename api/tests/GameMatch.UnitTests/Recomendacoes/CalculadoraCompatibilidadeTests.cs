using GameMatch.Domain.Recomendacoes;

namespace GameMatch.UnitTests.Recomendacoes;

public sealed class CalculadoraCompatibilidadeTests
{
    private static readonly PreferenciasCalculo PerfilCompleto =
        new(["RPG", "Ação"], ["PC (Windows)"], ["turn-based"]);

    [Fact]
    public void Calcular_com_os_tres_fatores_retorna_pontuacao_e_explicacao()
    {
        var jogo = new CaracteristicasJogo(
            ["RPG", "Ação"],
            ["PC (Windows)"],
            ["turn-based"]);

        var resultado = CalculadoraCompatibilidade.Calcular(PerfilCompleto, jogo);

        // Pesos renormalizados entre os três fatores definidos: 40/85, 20/85
        // e 25/85, todos com fator 1.
        Assert.Equal(100, resultado.Pontuacao);
        Assert.Equal(
            ["generos", "plataformas", "estilos"],
            resultado.Fatores.Select(fator => fator.Chave));
        Assert.Equal(
            [47, 24, 29],
            resultado.Fatores.Select(fator => fator.Contribuicao));
        Assert.Equal(
            resultado.Pontuacao,
            resultado.Fatores.Sum(fator => fator.Contribuicao));
    }

    [Fact]
    public void Calcular_sem_estilos_no_perfil_ignora_o_fator_e_renormaliza_os_pesos()
    {
        var preferencias = new PreferenciasCalculo(
            ["RPG", "Ação"],
            ["PC (Windows)"],
            []);

        var jogo = new CaracteristicasJogo(["RPG"], ["Web Browser"], []);

        var resultado = CalculadoraCompatibilidade.Calcular(preferencias, jogo);

        // Sem o fator de estilos os pesos 40% e 20% viram 40/60 e 20/60; com
        // gênero no jogo e plataforma diferente, só o gênero contribui.
        Assert.Equal(33, resultado.Pontuacao);
        Assert.Equal(
            ["generos", "plataformas"],
            resultado.Fatores.Select(fator => fator.Chave));
        Assert.Equal(33, resultado.Fatores[0].Contribuicao);
        Assert.Equal(0, resultado.Fatores[1].Contribuicao);
    }

    [Fact]
    public void Calcular_sem_nenhuma_correspondencia_retorna_zero()
    {
        var jogo = new CaracteristicasJogo(
            ["MOBA"],
            ["Web Browser"],
            ["pvp"]);

        var resultado = CalculadoraCompatibilidade.Calcular(PerfilCompleto, jogo);

        Assert.Equal(0, resultado.Pontuacao);
        Assert.All(resultado.Fatores, fator => Assert.Equal(0, fator.Contribuicao));
        Assert.All(
            resultado.Fatores,
            fator => Assert.Empty(fator.Correspondencias));
    }

    [Fact]
    public void Calcular_somente_inclui_experiencia_quando_o_alvo_e_informado()
    {
        var jogo = new CaracteristicasJogo(["RPG"], ["PC (Windows)"], ["open-world"]);

        var semExperiencia = CalculadoraCompatibilidade.Calcular(PerfilCompleto, jogo);
        var comExperiencia = CalculadoraCompatibilidade.Calcular(
            PerfilCompleto,
            jogo,
            ["open-world", "sci-fi"]);

        Assert.Equal(
            ["generos", "plataformas", "estilos"],
            semExperiencia.Fatores.Select(fator => fator.Chave));

        Assert.Equal(
            ["generos", "plataformas", "estilos", "experiencia"],
            comExperiencia.Fatores.Select(fator => fator.Chave));

        // 20 (1 de 2 gêneros do perfil) + 20 (plataforma) + 0 (sem a tag
        // turn-based) + 8 (15% × 1/2 do alvo, arredondado).
        Assert.Equal(48, comExperiencia.Pontuacao);
        Assert.Equal(8, comExperiencia.Fatores[3].Contribuicao);
        Assert.Equal(["open-world"], comExperiencia.Fatores[3].Correspondencias);
    }

    [Fact]
    public void Calcular_apresenta_fator_definido_sem_correspondencia_com_contribuicao_zero()
    {
        var jogo = new CaracteristicasJogo(["MOBA"], ["PC (Windows)"], ["pvp"]);

        var resultado = CalculadoraCompatibilidade.Calcular(PerfilCompleto, jogo);

        var estilos = Assert.Single(
            resultado.Fatores,
            fator => fator.Chave == "estilos");

        Assert.Equal(0, estilos.Contribuicao);
        Assert.Empty(estilos.Correspondencias);
    }

    [Fact]
    public void Calcular_mantem_a_grafia_do_jogo_nas_correspondencias()
    {
        var preferencias = new PreferenciasCalculo(["rpg"], ["pc (windows)"], []);
        var jogo = new CaracteristicasJogo(["RPG"], ["PC (Windows)"], []);

        var resultado = CalculadoraCompatibilidade.Calcular(preferencias, jogo);

        Assert.Equal(["RPG"], resultado.Fatores[0].Correspondencias);
        Assert.Equal(["PC (Windows)"], resultado.Fatores[1].Correspondencias);
        Assert.Equal(100, resultado.Pontuacao);
    }

    [Fact]
    public void Calcular_com_perfil_sem_grupos_retorna_zero_sem_fatores()
    {
        var preferencias = new PreferenciasCalculo([], [], []);
        var jogo = new CaracteristicasJogo(["RPG"], ["PC (Windows)"], ["turn-based"]);

        var resultado = CalculadoraCompatibilidade.Calcular(preferencias, jogo);

        Assert.Equal(0, resultado.Pontuacao);
        Assert.Empty(resultado.Fatores);
    }

    [Fact]
    public void Calcular_sempre_retorna_pontuacao_inteira_entre_zero_e_cem()
    {
        var casos = new[]
        {
            new CaracteristicasJogo(["RPG", "Ação"], ["PC (Windows)"], ["turn-based"]),
            new CaracteristicasJogo(["RPG"], ["PC (Windows)"], ["turn-based", "fantasia"]),
            new CaracteristicasJogo(["Shooter"], ["Web Browser"], ["pvp"]),
            new CaracteristicasJogo([], [], [])
        };

        foreach (var jogo in casos)
        {
            var resultado = CalculadoraCompatibilidade.Calcular(
                PerfilCompleto,
                jogo,
                ["explorar"]);

            Assert.InRange(resultado.Pontuacao, 0, 100);
            Assert.Equal(
                resultado.Pontuacao,
                resultado.Fatores.Sum(fator => fator.Contribuicao));
            Assert.All(
                resultado.Fatores,
                fator => Assert.InRange(fator.Contribuicao, 0, 100));
        }
    }
}
