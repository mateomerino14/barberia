using System.Security.Claims;
using System.Text;
using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Barberia.Infrastructure.Security;

/// <summary>Adaptador que emite pases de sesión JWT firmados.</summary>
public sealed class TokenIssuer(IOptions<JwtOptions> options, TimeProvider timeProvider) : ITokenIssuer
{
    private readonly JsonWebTokenHandler _handler = new();

    /// <summary>Emite un pase con el rol y la ficha de barbero de la cuenta.</summary>
    public IssuedToken Issue(StaffAccount account)
    {
        JwtOptions settings = options.Value;
        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset expires = now.AddHours(settings.LifetimeHours);
        List<Claim> claims =
        [
            new(StaffClaims.AccountId, account.Id.ToString()),
            new(StaffClaims.Role, account.Role.ToString()),
            new(StaffClaims.Email, account.Email),
        ];
        if (account.BarberId is not null)
        {
            claims.Add(new Claim(StaffClaims.BarberId, account.BarberId.Value.ToString()));
        }

        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = new SigningCredentials(SigningKey(settings), SecurityAlgorithms.HmacSha256),
        };
        return new IssuedToken(_handler.CreateToken(descriptor), expires);
    }

    /// <summary>Arma la clave de firma a partir de la configuración.</summary>
    public static SymmetricSecurityKey SigningKey(JwtOptions settings)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
    }
}
