using GameMatch.Application.Catalogo;

namespace GameMatch.UnitTests.Catalogo;

public sealed class ConsultaCatalogoTests
{
    [Fact]
    public void Criar_sem_parametros_usa_os_padroes()
    {
        var consulta = ConsultaCatalogo.Criar(null, null, null, null, null, null);

        Assert.Equal(1, consulta.Pagina);
        Assert.Equal(24, consulta.Tamanho);
        Assert.Null(consulta.Busca);
        Assert.Null(consulta.Genero);
        Assert.Null(consulta.Plataforma);
        Assert.Null(consulta.Tag);
    }

    [Fact]
    public void Criar_remove_espacos_e_trata_vazio_como_ausente()
    {
        var consulta = ConsultaCatalogo.Criar(
            2,
            50,
            "  aurora  ",
            "   ",
            "PC (Windows)",
            null);

        Assert.Equal(2, consulta.Pagina);
        Assert.Equal(50, consulta.Tamanho);
        Assert.Equal("aurora", consulta.Busca);
        Assert.Null(consulta.Genero);
        Assert.Equal("PC (Windows)", consulta.Plataforma);
        Assert.Null(consulta.Tag);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 100)]
    [InlineData(3, 24)]
    public void Validar_aceita_paginacao_dentro_dos_limites(int pagina, int tamanho)
    {
        var erros = new ConsultaCatalogo(pagina, tamanho).Validar();

        Assert.Empty(erros);
    }

    [Theory]
    [InlineData(0, 24, "pagina")]
    [InlineData(-1, 24, "pagina")]
    [InlineData(1, 0, "tamanho")]
    [InlineData(1, 101, "tamanho")]
    public void Validar_rejeita_paginacao_invalida(
        int pagina,
        int tamanho,
        string campo)
    {
        var erros = new ConsultaCatalogo(pagina, tamanho).Validar();

        Assert.True(erros.ContainsKey(campo));
        Assert.NotEmpty(erros[campo]);
    }
}
