namespace Barberia.Domain.Common;

/// <summary>Horario de un día de la semana: desde qué hora y hasta qué hora se atiende.</summary>
public sealed record DailyHours
{
    /// <summary>Constructor para la base de datos.</summary>
    private DailyHours()
    {
    }

    /// <summary>Día de la semana.</summary>
    public DayOfWeek Day { get; private init; }

    /// <summary>Hora de apertura.</summary>
    public TimeOnly Opens { get; private init; }

    /// <summary>Hora de cierre.</summary>
    public TimeOnly Closes { get; private init; }

    /// <summary>Crea el horario de un día verificando que abra antes de cerrar.</summary>
    public static Result<DailyHours> Create(DayOfWeek day, TimeOnly opens, TimeOnly closes)
    {
        if (opens >= closes)
        {
            return Error.Validation("hours.invalid", "La hora de apertura tiene que ser antes de la de cierre.");
        }

        return new DailyHours { Day = day, Opens = opens, Closes = closes };
    }

    /// <summary>Indica si este horario cubre por completo otro horario del mismo día.</summary>
    public bool Covers(DailyHours other)
    {
        return Day == other.Day && Opens <= other.Opens && Closes >= other.Closes;
    }

    /// <summary>Verifica que una lista de horarios no repita días.</summary>
    public static Result EnsureOnePerDay(IReadOnlyCollection<DailyHours> hours)
    {
        if (hours.Select(item => item.Day).Distinct().Count() != hours.Count)
        {
            return Error.Validation("hours.repeated_day", "Cada día de la semana puede tener un solo horario.");
        }

        return Result.Success();
    }
}
