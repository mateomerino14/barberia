using Barberia.Application.Ports;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Bookings;

/// <summary>Para qué se buscan los días: un servicio y un barbero, o ninguno de los dos todavía.</summary>
public sealed record GetDaySummaryQuery(Guid? ServiceId, Guid? BarberId);

/// <summary>Un día que se puede reservar: si la barbería abre y cuántas horas de inicio le quedan libres.</summary>
public sealed record DaySummary(DateOnly Date, bool Open, int FreeStarts);

/// <summary>Resume los días que se pueden reservar, para marcar los llenos antes de que el cliente los toque.</summary>
public sealed class GetDaySummaryHandler(
    IShopRepository shops,
    IServiceRepository services,
    IBarberRepository barbers,
    ScheduleReader schedule,
    IClock clock)
{
    /// <summary>Cuenta las horas libres del servicio, o del más corto de cada barbero si aún no se eligió: sin lugar para ese, el día está lleno.</summary>
    public async Task<Result<IReadOnlyList<DaySummary>>> HandleAsync(GetDaySummaryQuery query, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        IReadOnlyList<Service> active = await services.ListAsync(true, cancellationToken);
        Service? chosen = null;
        if (query.ServiceId is not null)
        {
            chosen = active.FirstOrDefault(service => service.Id == query.ServiceId);
            if (chosen is null)
            {
                return SchedulingErrors.ServiceUnavailable;
            }
        }

        Dictionary<Barber, int> durations = await DurationsAsync(active, chosen, query.BarberId, cancellationToken);
        DateOnly today = BusinessTime.DateOf(clock.Now);
        DateOnly last = today.AddDays(shop.Rules.MaxDaysAhead);
        TimeRange window = TimeRange.Create(ScheduleReader.DayRange(today).Start, ScheduleReader.DayRange(last).End).Value;
        ILookup<Guid, TimeRange> busy = await schedule.BusyByBarberAsync(window, cancellationToken);

        List<DaySummary> days = [];
        for (DateOnly date = today; date <= last; date = date.AddDays(1))
        {
            HashSet<DateTimeOffset> starts = [];
            foreach ((Barber barber, int minutes) in durations)
            {
                starts.UnionWith(AvailabilityCalculator.FreeStarts(shop, barber, date, minutes, busy[barber.Id], clock.Now));
            }

            days.Add(new DaySummary(date, shop.HoursOn(date) is not null, starts.Count));
        }

        return days;
    }

    /// <summary>Barberos que cuentan y la duración que se busca para cada uno.</summary>
    private async Task<Dictionary<Barber, int>> DurationsAsync(
        IReadOnlyList<Service> active,
        Service? chosen,
        Guid? barberId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Barber> team = await barbers.ListAsync(true, cancellationToken);
        Dictionary<Barber, int> durations = [];
        foreach (Barber barber in team.Where(barber => barberId is null || barber.Id == barberId))
        {
            List<int> offered = [.. active.Where(service => barber.Offers(service.Id) && (chosen is null || service.Id == chosen.Id))
                .Select(service => service.DurationMinutes)];
            if (offered.Count > 0)
            {
                durations[barber] = offered.Min();
            }
        }

        return durations;
    }
}
