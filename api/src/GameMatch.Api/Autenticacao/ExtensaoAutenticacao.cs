using System.Text;
using GameMatch.Domain.Entidades;
using GameMatch.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace GameMatch.Api.Autenticacao;

/// <summary>
/// Registra o Identity (com as regras de senha do projeto) e a autenticação
/// JWT usada pelos endpoints de conta.
/// </summary>
public static class ExtensaoAutenticacao
{
    public const string NomeEmissorPadrao = "GameMatch.Api";
    public const string NomeAudienciaPadrao = "GameMatch.Web";
    public const int ExpiracaoMinutosPadrao = 60;

    public static IServiceCollection AdicionarAutenticacaoJwt(
        this IServiceCollection services,
        IConfiguration configuracao)
    {
        var chave = configuracao["Jwt:Chave"];
        if (string.IsNullOrWhiteSpace(chave))
        {
            throw new InvalidOperationException(
                "A chave de assinatura 'Jwt:Chave' não foi configurada. " +
                "Defina a variável de ambiente Jwt__Chave ou use " +
                "dotnet user-secrets set \"Jwt:Chave\" \"<chave com pelo menos 32 caracteres>\".");
        }

        if (Encoding.UTF8.GetByteCount(chave) < 32)
        {
            throw new InvalidOperationException(
                "A chave 'Jwt:Chave' precisa ter pelo menos 32 bytes para assinar " +
                "tokens com HMAC-SHA256.");
        }

        var emissor = configuracao["Jwt:Emissor"] ?? NomeEmissorPadrao;
        var audiencia = configuracao["Jwt:Audience"] ?? NomeAudienciaPadrao;

        services
            .AddIdentityCore<Usuario>(opcoes =>
            {
                opcoes.Password.RequiredLength = 8;
                opcoes.Password.RequireDigit = true;
                opcoes.Password.RequireUppercase = true;
                opcoes.Password.RequireLowercase = true;
                opcoes.Password.RequireNonAlphanumeric = false;
                opcoes.Password.RequiredUniqueChars = 1;
                opcoes.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<GameMatchDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opcoes =>
            {
                opcoes.MapInboundClaims = false;
                opcoes.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = emissor,
                    ValidateAudience = true,
                    ValidAudience = audiencia,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chave)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        services.AddAuthorization();

        return services;
    }
}
