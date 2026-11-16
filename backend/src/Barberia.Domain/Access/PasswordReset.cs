using System.Security.Cryptography;
using System.Text;
using Barberia.Domain.Common;

namespace Barberia.Domain.Access;

/// <summary>Enlace para crear una contraseña nueva: dura poco, sirve una vez y solo se guarda su huella.</summary>
public sealed class PasswordReset : Entity
{
    /// <summary>Minutos que dura el enlace.</summary>
    public const int LifetimeMinutes = 30;

    private const int TokenBytes = 32;

    /// <summary>Constructor para la base de datos.</summary>
    private PasswordReset()
    {
    }

    /// <summary>Cuenta a la que pertenece el enlace.</summary>
    public Guid AccountId { get; private init; }

    /// <summary>Huella SHA-256 de la clave del enlace; la clave misma nunca se guarda.</summary>
    public string TokenHash { get; private init; } = string.Empty;

    /// <summary>Cuándo deja de servir.</summary>
    public DateTimeOffset ExpiresAt { get; private init; }

    /// <summary>Cuándo se usó; vacío si todavía no.</summary>
    public DateTimeOffset? UsedAt { get; private set; }

    /// <summary>Crea un enlace nuevo y devuelve también su clave, que solo viaja en el correo.</summary>
    public static PasswordResetLink Issue(Guid accountId, DateTimeOffset now)
    {
        string token = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(TokenBytes));
        PasswordReset reset = new()
        {
            AccountId = accountId,
            TokenHash = Fingerprint(token),
            ExpiresAt = now.ToUniversalTime().AddMinutes(LifetimeMinutes),
        };
        return new PasswordResetLink(reset, token);
    }

    /// <summary>Huella con que se busca un enlace a partir de su clave.</summary>
    public static string Fingerprint(string token)
    {
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    /// <summary>Indica si el enlace todavía sirve: no se usó y no venció.</summary>
    public bool IsUsable(DateTimeOffset now)
    {
        return UsedAt is null && now < ExpiresAt;
    }

    /// <summary>Anota que el enlace ya se usó o que quedó anulado.</summary>
    public void MarkUsed(DateTimeOffset now)
    {
        UsedAt ??= now.ToUniversalTime();
    }
}

/// <summary>Enlace recién creado junto con su clave en claro.</summary>
public sealed record PasswordResetLink(PasswordReset Reset, string Token);
