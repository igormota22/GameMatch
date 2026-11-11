using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GameMatch.Domain.Entidades;
using Microsoft.IdentityModel.Tokens;

namespace GameMatch.Api.Autenticacao;

public sealed record TokenGerado(string Token, DateTimeOffset ExpiraEm);

/// <summary>
/// Emite o token JWT assinado a partir da configuração `Jwt:*`.
/// </summary>
public sealed class GeradorTokenJwt(IConfiguration configuracao)
{
    public TokenGerado Gerar(Usuario usuario)
    {
        var chave = configuracao["Jwt:Chave"]
            ?? throw new InvalidOperationException("A chave 'Jwt:Chave' não está configurada.");

        var emissor = configuracao["Jwt:Emissor"] ?? ExtensaoAutenticacao.NomeEmissorPadrao;
        var audiencia = configuracao["Jwt:Audience"] ?? ExtensaoAutenticacao.NomeAudienciaPadrao;
        var expiracaoMinutos = configuracao.GetValue<int?>("Jwt:ExpiracaoMinutos")
            ?? ExtensaoAutenticacao.ExpiracaoMinutosPadrao;

        var expiraEm = DateTimeOffset.UtcNow.AddMinutes(expiracaoMinutos);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id),
            new(JwtRegisteredClaimNames.Email, usuario.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, usuario.UserName ?? usuario.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credenciais = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: emissor,
            audience: audiencia,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiraEm.UtcDateTime,
            signingCredentials: credenciais);

        return new TokenGerado(new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
