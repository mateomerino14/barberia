using System.Net.Mail;

namespace Barberia.Domain.Common;

/// <summary>Normaliza y valida correos electrónicos.</summary>
public static class EmailAddress
{
    private static readonly Error InvalidEmail = Error.Validation("email.invalid", "El correo no es válido.");

    /// <summary>Devuelve el correo en minúsculas si tiene forma válida.</summary>
    public static Result<string> Normalize(string? value)
    {
        Result<string> text = Text.Required(value, Limits.EmailMaxLength, InvalidEmail);
        if (text.IsFailure)
        {
            return InvalidEmail;
        }

        string lower = text.Value.ToLowerInvariant();
        if (!MailAddress.TryCreate(lower, out MailAddress? parsed) || parsed.Address != lower)
        {
            return InvalidEmail;
        }

        return lower;
    }
}
