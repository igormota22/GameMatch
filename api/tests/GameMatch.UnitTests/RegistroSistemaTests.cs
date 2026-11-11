using GameMatch.Domain.Entidades;

namespace GameMatch.UnitTests;

public sealed class RegistroSistemaTests
{
    [Fact]
    public void RegistroSistema_inicia_com_campos_vazios()
    {
        var registro = new RegistroSistema();

        Assert.Equal(string.Empty, registro.Chave);
        Assert.Equal(string.Empty, registro.Valor);
        Assert.Equal(default, registro.AtualizadoEm);
    }
}
