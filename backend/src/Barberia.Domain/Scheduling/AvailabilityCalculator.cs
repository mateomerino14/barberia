using Barberia.Domain.Common;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Domain.Scheduling;

/// <summary>Calcula en qué horas entra un servicio completo en la agenda de un barbero.</summary>
public static class AvailabilityCalculator
{
    /// <summary>Devuelve los inicios libres de un barbero en una fecha para un servicio de la duración dada.</summary>
    public static IReadOnlyList<DateTimeOffset> FreeStarts(
        Shop shop,
        Barber barber,
        DateOnly date,
        int durationMinutes,
        IEnumerable<TimeRange> busy,
        DateTimeOffset now)
    {
        TimeRange? workday = Workday(shop, barber, date);
        if (workday is null || !IsWithinBookingWindow(shop.Rules, date, now))
        {
            return [];
        }

        DateTimeOffset earliest = now.ToUniversalTime().AddMinutes(shop.Rules.MinLeadMinutes);
        List<TimeRange> taken = [.. busy];
        List<DateTimeOffset> starts = [];
        int step = shop.Rules.SlotStepMinutes;
        for (DateTimeOffset start = workday.Start; start.AddMinutes(durationMinutes) <= workday.End; start = start.AddMinutes(step))
        {
            TimeRange candidate = TimeRange.FromStart(start, durationMinutes);
            if (start >= earliest && !taken.Any(range => range.Overlaps(candidate)))
            {
                starts.Add(start);
            }
        }

        return starts;
    }

    /// <summary>Indica si un inicio concreto está libre, con las mismas reglas que la lista de horas.</summary>
    public static bool IsFree(
        Shop shop,
        Barber barber,
        DateTimeOffset start,
        int durationMinutes,
        IEnumerable<TimeRange> busy,
        DateTimeOffset now)
    {
        DateOnly date = BusinessTime.DateOf(start);
        return FreeStarts(shop, barber, date, durationMinutes, busy, now).Contains(start.ToUniversalTime());
    }

    /// <summary>Indica si un turno anotado por la barbería cabe en el día del barbero sin chocar, sin reglas de anticipación.</summary>
    public static bool FitsWorkday(Shop shop, Barber barber, TimeRange range, IEnumerable<TimeRange> busy)
    {
        TimeRange? workday = Workday(shop, barber, BusinessTime.DateOf(range.Start));
        return workday is not null && range.IsWithin(workday) && !busy.Any(item => item.Overlaps(range));
    }

    /// <summary>Devuelve el horario de trabajo del barbero en la fecha, recortado al de la barbería, o null si no trabaja.</summary>
    public static TimeRange? Workday(Shop shop, Barber barber, DateOnly date)
    {
        DailyHours? shopHours = shop.HoursOn(date);
        DailyHours? barberHours = barber.HoursOn(date);
        if (shopHours is null || barberHours is null || !barber.IsActive)
        {
            return null;
        }

        TimeOnly opens = barberHours.Opens;
        if (shopHours.Opens > opens)
        {
            opens = shopHours.Opens;
        }

        TimeOnly closes = barberHours.Closes;
        if (shopHours.Closes < closes)
        {
            closes = shopHours.Closes;
        }

        Result<TimeRange> range = TimeRange.Create(BusinessTime.At(date, opens), BusinessTime.At(date, closes));
        if (range.IsFailure)
        {
            return null;
        }

        return range.Value;
    }

    /// <summary>Indica si la fecha está entre hoy y el último día que se puede reservar.</summary>
    private static bool IsWithinBookingWindow(BookingRules rules, DateOnly date, DateTimeOffset now)
    {
        DateOnly today = BusinessTime.DateOf(now);
        return date >= today && date <= today.AddDays(rules.MaxDaysAhead);
    }
}
