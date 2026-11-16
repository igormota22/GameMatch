namespace GameMatch.Domain.Entidades;

/// <summary>
/// Característica de um jogo do catálogo. A chave composta por jogo, tipo e
/// valor garante que a mesma característica não seja persistida duas vezes.
/// </summary>
public sealed class JogoCaracteristica
{
    public int JogoId { get; set; }

    public TipoCaracteristica Tipo { get; set; }

    public string Valor { get; set; } = string.Empty;
}
