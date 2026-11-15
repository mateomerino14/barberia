namespace Barberia.Domain.Common;

/// <summary>Zona horaria en la que trabaja la barbería y en la que se leen todas las horas.</summary>
public static class BusinessTime
{
    /// <summary>Identificador IANA de la hora de Bolivia.</summary>
    public const string TimeZoneId = "America/La_Paz";

    /// <summary>Devuelve la zona horaria de la barbería.</summary>
    public static TimeZoneInfo Zone()
    {
        return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    }

    /// <summary>Devuelve en UTC el instante que corresponde a una fecha y hora locales de la barbería.</summary>
    public static DateTimeOffset At(DateOnly date, TimeOnly time)
    {
        DateTime local = date.ToDateTime(time, DateTimeKind.Unspecified);
        TimeSpan offset = Zone().GetUtcOffset(local);
        return new DateTimeOffset(local, offset).ToUniversalTime();
    }

    /// <summary>Devuelve la fecha local de la barbería en la que cae un instante.</summary>
    public static DateOnly DateOf(DateTimeOffset instant)
    {
        return DateOnly.FromDateTime(ToLocal(instant).DateTime);
    }

    /// <summary>Convierte un instante a la hora local de la barbería.</summary>
    public static DateTimeOffset ToLocal(DateTimeOffset instant)
    {
        return TimeZoneInfo.ConvertTime(instant, Zone());
    }
}
