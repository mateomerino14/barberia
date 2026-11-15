using Barberia.Domain.Common;

namespace Barberia.Domain.Scheduling;

/// <summary>Datos de contacto de quien reserva.</summary>
public sealed record Customer
{
    private const int MinPhoneDigits = 7;
    private const int MaxPhoneDigits = 15;

    /// <summary>Constructor para la base de datos.</summary>
    private Customer()
    {
    }

    /// <summary>Nombre del cliente.</summary>
    public string Name { get; private init; } = string.Empty;

    /// <summary>Celular solo con dígitos, o vacío en un turno sin reserva.</summary>
    public string Phone { get; private init; } = string.Empty;

    /// <summary>Correo para la confirmación, o vacío.</summary>
    public string Email { get; private init; } = string.Empty;

    /// <summary>Crea los datos del cliente; el celular es obligatorio salvo que se diga lo contrario.</summary>
    public static Result<Customer> Create(string? name, string? phone, string? email, bool phoneRequired)
    {
        Result<string> validName = Text.Required(name, Limits.NameMaxLength, SchedulingErrors.InvalidCustomerName);
        if (validName.IsFailure)
        {
            return validName.Error!;
        }

        string digits = new([.. (phone ?? string.Empty).Where(char.IsDigit)]);
        bool phoneMissing = digits.Length == 0;
        if ((phoneRequired || !phoneMissing) && (digits.Length < MinPhoneDigits || digits.Length > MaxPhoneDigits))
        {
            return SchedulingErrors.InvalidPhone;
        }

        string validEmail = string.Empty;
        if (!string.IsNullOrWhiteSpace(email))
        {
            Result<string> normalized = EmailAddress.Normalize(email);
            if (normalized.IsFailure)
            {
                return normalized.Error!;
            }

            validEmail = normalized.Value;
        }

        return new Customer { Name = validName.Value, Phone = digits, Email = validEmail };
    }
}
