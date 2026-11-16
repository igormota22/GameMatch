using GameMatch.Domain.Preferencias;

namespace GameMatch.UnitTests.Preferencias;

public sealed class NormalizadorPreferenciasTests
{
    [Fact]
    public void Normalizar_apara_e_colapsa_espacos_internos()
    {
        var resultado = NormalizadorPreferencias.Normalizar("  RPG   de   ação  ");

        Assert.Equal("RPG de ação", resultado);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalizar_valor_vazio_retorna_texto_vazio(string? valor)
    {
        Assert.Equal(string.Empty, NormalizadorPreferencias.Normalizar(valor));
    }

    [Fact]
    public void NormalizarLista_deduplica_sem_diferenciar_maiusculas()
    {
        var resultado = NormalizadorPreferencias.NormalizarLista(
            ["RPG", " rpg ", "Ação", "AÇÃO", "  "]);

        Assert.Equal(["RPG", "Ação"], resultado);
    }

    [Fact]
    public void NormalizarLista_preserva_a_primeira_grafia()
    {
        var resultado = NormalizadorPreferencias.NormalizarLista(["PC (Windows)", "pc (windows)"]);

        Assert.Equal(["PC (Windows)"], resultado);
    }

    [Fact]
    public void NormalizarLista_com_entrada_nula_retorna_lista_vazia()
    {
        Assert.Empty(NormalizadorPreferencias.NormalizarLista(null));
    }

    [Fact]
    public void TamanhoMaximoValor_e_de_cem_caracteres()
    {
        Assert.Equal(100, NormalizadorPreferencias.TamanhoMaximoValor);
    }
}
