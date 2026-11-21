namespace GameMatch.Domain.Entidades;

/// <summary>
/// Avaliação de um jogo pelo usuário, com uma nota inteira de 1 a 5 e no máximo
/// um registro por par (usuario, jogo) — RB-15 e INV-04/INV-05.
/// </summary>
public sealed class Avaliacao
{
    public const int NotaMinima = 1;

    public const int NotaMaxima = 5;

    public string UsuarioId { get; set; } = string.Empty;

    public int JogoId { get; set; }

    public int Nota { get; set; }

    public DateTimeOffset AtualizadaEm { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// A nota só é válida entre 1 e 5 estrelas (INV-04).
    /// </summary>
    public bool NotaValida => Nota is >= NotaMinima and <= NotaMaxima;

    public static bool ValidarNota(int nota) =>
        nota is >= NotaMinima and <= NotaMaxima;
}
