using Barberia.Domain.Common;

namespace Barberia.Domain.Shops;

/// <summary>Reglas de reserva de la barbería: anticipación mínima, días hacia adelante y plazo para cancelar.</summary>
public sealed record BookingRules
{
    private const int DefaultMinLeadMinutes = 60;
    private const int DefaultMaxDaysAhead = 14;
    private const int DefaultCancelLimitMinutes = 120;
    private const int MaxDaysAheadLimit = 90;
    private const int MaxCancelLimitMinutes = 2 * Limits.MinutesPerDay;

    /// <summary>Constructor para la base de datos.</summary>
    private BookingRules()
    {
    }

    /// <summary>Minutos de anticipación mínima para reservar.</summary>
    public int MinLeadMinutes { get; private init; }

    /// <summary>Cuántos días hacia adelante se puede reservar.</summary>
    public int MaxDaysAhead { get; private init; }

    /// <summary>Hasta cuántos minutos antes del turno el cliente puede cancelar o cambiar.</summary>
    public int CancelLimitMinutes { get; private init; }

    /// <summary>Reglas con que empieza una barbería nueva.</summary>
    public static BookingRules Default { get; } = new()
    {
        MinLeadMinutes = DefaultMinLeadMinutes,
        MaxDaysAhead = DefaultMaxDaysAhead,
        CancelLimitMinutes = DefaultCancelLimitMinutes,
    };

    /// <summary>Crea reglas verificando que estén dentro de rangos razonables.</summary>
    public static Result<BookingRules> Create(int minLeadMinutes, int maxDaysAhead, int cancelLimitMinutes)
    {
        if (minLeadMinutes < 0 || minLeadMinutes > Limits.MinutesPerDay)
        {
            return Error.Validation("rules.min_lead", "La anticipación mínima va de 0 minutos a 1 día.");
        }

        if (maxDaysAhead < 1 || maxDaysAhead > MaxDaysAheadLimit)
        {
            return Error.Validation("rules.max_days", "Se puede reservar de 1 a 90 días hacia adelante.");
        }

        if (cancelLimitMinutes < 0 || cancelLimitMinutes > MaxCancelLimitMinutes)
        {
            return Error.Validation("rules.cancel_limit", "El plazo para cancelar va de 0 minutos a 2 días.");
        }

        return new BookingRules
        {
            MinLeadMinutes = minLeadMinutes,
            MaxDaysAhead = maxDaysAhead,
            CancelLimitMinutes = cancelLimitMinutes,
        };
    }
}
