using Barberia.Domain.Common;

namespace Barberia.Domain.Staff;

/// <summary>Días en los que un barbero no atiende, por vacaciones u otro motivo.</summary>
public sealed class TimeOff : Entity
{
    /// <summary>Constructor para la base de datos.</summary>
    private TimeOff()
    {
    }

    /// <summary>Primer día de la ausencia.</summary>
    public DateOnly From { get; private init; }

    /// <summary>Último día de la ausencia, incluido.</summary>
    public DateOnly To { get; private init; }

    /// <summary>Motivo interno; el cliente no lo ve.</summary>
    public string Reason { get; private init; } = string.Empty;

    /// <summary>Crea una ausencia verificando que termine el mismo día o después de empezar.</summary>
    public static Result<TimeOff> Create(DateOnly from, DateOnly to, string? reason)
    {
        if (to < from)
        {
            return StaffErrors.InvalidTimeOff;
        }

        Result<string> validReason = Text.Optional(reason, Limits.ReasonMaxLength, StaffErrors.InvalidReason);
        if (validReason.IsFailure)
        {
            return validReason.Error!;
        }

        return new TimeOff { From = from, To = to, Reason = validReason.Value };
    }

    /// <summary>Indica si la ausencia incluye la fecha.</summary>
    public bool Includes(DateOnly date)
    {
        return date >= From && date <= To;
    }
}
