using Barberia.Domain.Common;

namespace Barberia.Domain.Access;

/// <summary>Errores de negocio del acceso al panel.</summary>
public static class AccessErrors
{
    /// <summary>El correo o la contraseña no coinciden.</summary>
    public static readonly Error InvalidCredentials = Error.Unauthorized("access.invalid_credentials", "El correo o la contraseña no son correctos.");

    /// <summary>La contraseña es muy corta o muy larga.</summary>
    public static readonly Error WeakPassword = Error.Validation("access.weak_password", "La contraseña tiene que tener de 8 a 72 caracteres.");

    /// <summary>Ya hay una cuenta con ese correo.</summary>
    public static readonly Error EmailTaken = Error.Conflict("access.email_taken", "Ya hay una cuenta con ese correo.");

    /// <summary>Un barbero necesita estar vinculado a su ficha.</summary>
    public static readonly Error BarberRequired = Error.Validation("access.barber_required", "La cuenta de un barbero tiene que estar vinculada a su ficha.");
}
