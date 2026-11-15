using System.Security.Cryptography;
using System.Text;
using Barberia.Domain.Common;

namespace Barberia.Domain.Access;

/// <summary>Cuenta con la que el dueño o un barbero entran al panel.</summary>
public sealed class StaffAccount : Entity
{
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 72;
    private const int StampLength = 16;

    /// <summary>Constructor para la base de datos.</summary>
    private StaffAccount()
    {
    }

    /// <summary>Correo en minúsculas con el que entra.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Contraseña protegida; nunca se guarda en texto.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Rol en el panel.</summary>
    public StaffRole Role { get; private init; }

    /// <summary>Ficha de barbero vinculada; el dueño puede no atender.</summary>
    public Guid? BarberId { get; private init; }

    /// <summary>Huella de la contraseña actual: al cambiarla, las sesiones abiertas dejan de valer.</summary>
    public string SessionStamp => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(PasswordHash)))[..StampLength];

    /// <summary>Crea una cuenta con la contraseña ya protegida.</summary>
    public static Result<StaffAccount> Create(string? email, string passwordHash, StaffRole role, Guid? barberId)
    {
        Result<string> validEmail = EmailAddress.Normalize(email);
        if (validEmail.IsFailure)
        {
            return validEmail.Error!;
        }

        if (role == StaffRole.Barber && barberId is null)
        {
            return AccessErrors.BarberRequired;
        }

        return new StaffAccount { Email = validEmail.Value, PasswordHash = passwordHash, Role = role, BarberId = barberId };
    }

    /// <summary>Verifica que una contraseña nueva tenga un largo aceptable.</summary>
    public static Result ValidatePassword(string? password)
    {
        int length = (password ?? string.Empty).Length;
        if (length < MinPasswordLength || length > MaxPasswordLength)
        {
            return AccessErrors.WeakPassword;
        }

        return Result.Success();
    }

    /// <summary>Reemplaza la contraseña protegida.</summary>
    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
