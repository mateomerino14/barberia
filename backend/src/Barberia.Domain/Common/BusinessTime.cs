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

    /// <summary>Convierte un instante a la hora local de la barbería.</summary>
    public static DateTimeOffset ToLocal(DateTimeOffset instant)
    {
        return TimeZoneInfo.ConvertTime(instant, Zone());
    }
}
