namespace Barberia.Domain.Common;

/// <summary>Error de negocio previsto, con un código estable y un mensaje en español para la persona.</summary>
public sealed record Error(ErrorKind Kind, string Code, string Message)
{
    /// <summary>Crea un error de validación.</summary>
    public static Error Validation(string code, string message)
    {
        return new Error(ErrorKind.Validation, code, message);
    }

    /// <summary>Crea un error de algo que no existe.</summary>
    public static Error NotFound(string code, string message)
    {
        return new Error(ErrorKind.NotFound, code, message);
    }

    /// <summary>Crea un error de conflicto con el estado actual.</summary>
    public static Error Conflict(string code, string message)
    {
        return new Error(ErrorKind.Conflict, code, message);
    }

    /// <summary>Crea un error de credenciales inválidas.</summary>
    public static Error Unauthorized(string code, string message)
    {
        return new Error(ErrorKind.Unauthorized, code, message);
    }
}
