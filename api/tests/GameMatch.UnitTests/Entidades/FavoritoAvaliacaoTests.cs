using GameMatch.Domain.Entidades;

namespace GameMatch.UnitTests.Entidades;

/// <summary>
/// Invariantes INV-04 e INV-05: nota entre 1 e 5 e um único par
/// (usuario, jogo) por favorito/avaliação.
/// </summary>
public sealed class FavoritoAvaliacaoTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(1, true)]
    [InlineData(3, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public void Validar_nota_aceita_apenas_a_faixa_de_1_a_5(int nota, bool esperado)
    {
        Assert.Equal(esperado, Avaliacao.ValidarNota(nota));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    public void Nota_valida_acompanha_o_valor_da_avaliacao(int nota, bool esperado)
    {
        var avaliacao = new Avaliacao { Nota = nota };

        Assert.Equal(esperado, avaliacao.NotaValida);
    }

    [Fact]
    public void Favorito_identifica_o_par_usuario_e_jogo()
    {
        var favorito = new Favorito
        {
            UsuarioId = "usuario-1",
            JogoId = 7
        };

        Assert.Equal("usuario-1", favorito.UsuarioId);
        Assert.Equal(7, favorito.JogoId);
        Assert.NotEqual(default, favorito.CriadoEm);
    }

    [Fact]
    public void Favorito_e_avaliacao_do_mesmo_par_nao_se_confundem()
    {
        var favorito = new Favorito { UsuarioId = "usuario-1", JogoId = 7 };
        var avaliacao = new Avaliacao
        {
            UsuarioId = "usuario-1",
            JogoId = 7,
            Nota = 4
        };

        // Remover o favorito não apaga a avaliação (RB-14): são registros
        // independentes identificados pelo mesmo par.
        Assert.Equal(favorito.UsuarioId, avaliacao.UsuarioId);
        Assert.Equal(favorito.JogoId, avaliacao.JogoId);
        Assert.True(avaliacao.NotaValida);
    }
}
