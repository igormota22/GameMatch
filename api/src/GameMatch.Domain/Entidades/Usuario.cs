using Microsoft.AspNetCore.Identity;

namespace GameMatch.Domain.Entidades;

/// <summary>
/// Conta de usuário da plataforma. Herda de <see cref="IdentityUser"/> para
/// reaproveitar hash de senha, normalização de e-mail e bloqueio do Identity,
/// mantendo as regras de conta no domínio.
/// </summary>
public sealed class Usuario : IdentityUser
{
    /// <summary>
    /// Data de criação da conta, usada para exibição e ordenação futura.
    /// </summary>
    public DateTimeOffset CriadoEm { get; set; } = DateTimeOffset.UtcNow;
}
