namespace Barberia.Infrastructure.Security;

/// <summary>Nombres de los datos que viajan dentro del pase de sesión.</summary>
public static class StaffClaims
{
    /// <summary>Id de la cuenta.</summary>
    public const string AccountId = "sub";

    /// <summary>Rol de la cuenta.</summary>
    public const string Role = "role";

    /// <summary>Id de la ficha de barbero vinculada.</summary>
    public const string BarberId = "barber_id";

    /// <summary>Correo de la cuenta.</summary>
    public const string Email = "email";

    /// <summary>Huella de la contraseña con la que se abrió la sesión.</summary>
    public const string Stamp = "stamp";
}
