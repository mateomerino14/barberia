using Barberia.Domain.Common;

namespace Barberia.Domain.Scheduling;

/// <summary>Intervalo de tiempo en UTC que empieza en Start y termina justo antes de End.</summary>
public sealed record TimeRange
{
    /// <summary>Constructor para la base de datos.</summary>
    private TimeRange()
    {
    }

    /// <summary>Inicio del intervalo, en UTC.</summary>
    public DateTimeOffset Start { get; private init; }

    /// <summary>Fin del intervalo, en UTC; no se incluye.</summary>
    public DateTimeOffset End { get; private init; }

    /// <summary>Crea un intervalo verificando que termine después de empezar.</summary>
    public static Result<TimeRange> Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
        {
            return Error.Validation("time_range.invalid", "La hora de fin tiene que ser después de la de inicio.");
        }

        return new TimeRange { Start = start.ToUniversalTime(), End = end.ToUniversalTime() };
    }

    /// <summary>Crea un intervalo desde un inicio y una duración en minutos.</summary>
    public static TimeRange FromStart(DateTimeOffset start, int minutes)
    {
        return new TimeRange { Start = start.ToUniversalTime(), End = start.ToUniversalTime().AddMinutes(minutes) };
    }

    /// <summary>Indica si comparte algún momento con otro intervalo; tocarse en un borde no cuenta.</summary>
    public bool Overlaps(TimeRange other)
    {
        return Start < other.End && other.Start < End;
    }

    /// <summary>Indica si el intervalo cabe completo dentro de otro.</summary>
    public bool IsWithin(TimeRange other)
    {
        return Start >= other.Start && End <= other.End;
    }
}
