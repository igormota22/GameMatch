namespace GameMatch.Domain.Entidades;

/// <summary>
/// Jogo favoritado por um usuário. A chave composta impede duplicados e não há
/// limite de favoritos por usuário (RB-14, INV-05).
/// </summary>
public sealed class Favorito
{
    public string UsuarioId { get; set; } = string.Empty;

    public int JogoId { get; set; }

    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
}
