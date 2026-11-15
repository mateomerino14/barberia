namespace Barberia.Domain.Common;

/// <summary>Validaciones de texto que comparten todos los módulos.</summary>
public static class Text
{
    /// <summary>Recorta el texto y verifica que no esté vacío ni pase del largo máximo.</summary>
    public static Result<string> Required(string? value, int maxLength, Error error)
    {
        string trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0 || trimmed.Length > maxLength)
        {
            return error;
        }

        return trimmed;
    }

    /// <summary>Recorta un texto que puede quedar vacío y verifica que no pase del largo máximo.</summary>
    public static Result<string> Optional(string? value, int maxLength, Error error)
    {
        string trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length > maxLength)
        {
            return error;
        }

        return trimmed;
    }
}
