using Barberia.Domain.Common;

namespace Barberia.Domain.Shops;

/// <summary>Reglas de reserva de la barbería: anticipación, días hacia adelante, plazo para cancelar y cada cuánto empiezan los turnos.</summary>
public sealed record BookingRules
{
    private const int DefaultMinLeadMinutes = 60;
    private const int DefaultMaxDaysAhead = 14;
    private const int DefaultCancelLimitMinutes = 120;
    private const int MaxDaysAheadLimit = 90;
    private const int MaxCancelLimitMinutes = 2 * Limits.MinutesPerDay;
    private const int QuarterHour = 15;
    private const int TwentyMinutes = 20;
    private const int HalfHour = 30;
    private const int OneHour = 60;

    /// <summary>Intervalos que se pueden elegir para el inicio de los turnos.</summary>
    public static readonly IReadOnlyList<int> AllowedSlotSteps = [QuarterHour, TwentyMinutes, HalfHour, OneHour];

    /// <summary>Intervalo con que empieza una barbería nueva.</summary>
    public const int DefaultSlotStepMinutes = QuarterHour;

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

    /// <summary>Cada cuántos minutos se ofrece una hora de inicio: 15, 20, 30 o 60.</summary>
    public int SlotStepMinutes { get; private init; } = DefaultSlotStepMinutes;

    /// <summary>Reglas con que empieza una barbería nueva.</summary>
    public static BookingRules Default { get; } = new()
    {
        MinLeadMinutes = DefaultMinLeadMinutes,
        MaxDaysAhead = DefaultMaxDaysAhead,
        CancelLimitMinutes = DefaultCancelLimitMinutes,
        SlotStepMinutes = DefaultSlotStepMinutes,
    };

    /// <summary>Crea reglas verificando que estén dentro de rangos razonables.</summary>
    public static Result<BookingRules> Create(int minLeadMinutes, int maxDaysAhead, int cancelLimitMinutes, int slotStepMinutes)
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

        if (!AllowedSlotSteps.Contains(slotStepMinutes))
        {
            return Error.Validation("rules.slot_step", "Los turnos pueden empezar cada 15, 20, 30 o 60 minutos.");
        }

        return new BookingRules
        {
            MinLeadMinutes = minLeadMinutes,
            MaxDaysAhead = maxDaysAhead,
            CancelLimitMinutes = cancelLimitMinutes,
            SlotStepMinutes = slotStepMinutes,
        };
    }
}
