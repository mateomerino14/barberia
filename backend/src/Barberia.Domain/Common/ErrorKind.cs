namespace Barberia.Domain.Common;

/// <summary>Tipo de error de negocio, que la API traduce a su código HTTP.</summary>
public enum ErrorKind
{
    /// <summary>Los datos no cumplen una regla.</summary>
    Validation,

    /// <summary>Lo que se busca no existe.</summary>
    NotFound,

    /// <summary>La operación choca con el estado actual.</summary>
    Conflict,

    /// <summary>Las credenciales no son válidas.</summary>
    Unauthorized,

    /// <summary>La cuenta no tiene permiso sobre eso.</summary>
    Forbidden,
}
