using Barberia.Domain.Common;

namespace Barberia.Domain.Scheduling;

/// <summary>Horas en las que un barbero no atiende, como el almuerzo o un trámite.</summary>
public sealed class Block : Entity
{
    /// <summary>Constructor para la base de datos.</summary>
    private Block()
    {
    }

    /// <summary>Barbero bloqueado.</summary>
    public Guid BarberId { get; private init; }

    /// <summary>Horario bloqueado en UTC.</summary>
    public TimeRange Range { get; private init; } = null!;

    /// <summary>Motivo interno.</summary>
    public string Reason { get; private init; } = string.Empty;

    /// <summary>Crea un bloqueo.</summary>
    public static Result<Block> Create(Guid barberId, TimeRange range, string? reason)
    {
        Result<string> validReason = Text.Optional(reason, Limits.ReasonMaxLength, SchedulingErrors.InvalidReason);
        if (validReason.IsFailure)
        {
            return validReason.Error!;
        }

        return new Block { BarberId = barberId, Range = range, Reason = validReason.Value };
    }
}
