using Barberia.Application.Ports;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Bookings;

/// <summary>Qué horas libres se buscan: día, servicio y, si se eligió, barbero.</summary>
public sealed record GetAvailabilityQuery(DateOnly Date, Guid ServiceId, Guid? BarberId);

/// <summary>Hora libre y los barberos que pueden atender en ella.</summary>
public sealed record FreeSlot(DateTimeOffset Start, IReadOnlyList<Guid> BarberIds);

/// <summary>Calcula las horas libres de un día para un servicio, con un barbero o con cualquiera.</summary>
public sealed class GetAvailabilityHandler(
    IShopRepository shops,
    IServiceRepository services,
    IBarberRepository barbers,
    ScheduleReader schedule,
    IClock clock)
{
    /// <summary>Devuelve las horas libres ordenadas, cada una con sus barberos posibles.</summary>
    public async Task<Result<IReadOnlyList<FreeSlot>>> HandleAsync(GetAvailabilityQuery query, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        Service? service = await services.GetAsync(query.ServiceId, cancellationToken);
        if (service is null || !service.IsActive)
        {
            return SchedulingErrors.ServiceUnavailable;
        }

        IReadOnlyList<Barber> candidates = await Candidates(service, query.BarberId, cancellationToken);
        if (query.BarberId is not null && candidates.Count == 0)
        {
            return SchedulingErrors.BarberUnavailable;
        }

        SortedDictionary<DateTimeOffset, List<Guid>> slots = [];
        foreach (Barber barber in candidates)
        {
            IReadOnlyList<TimeRange> busy = await schedule.BusyAsync(barber.Id, query.Date, null, cancellationToken);
            foreach (DateTimeOffset start in AvailabilityCalculator.FreeStarts(shop, barber, query.Date, service.DurationMinutes, busy, clock.Now))
            {
                if (!slots.TryGetValue(start, out List<Guid>? ids))
                {
                    ids = [];
                    slots[start] = ids;
                }

                ids.Add(barber.Id);
            }
        }

        return slots.Select(pair => new FreeSlot(pair.Key, pair.Value)).ToList();
    }

    /// <summary>Devuelve el barbero elegido, o todos los activos que hacen el servicio.</summary>
    private async Task<IReadOnlyList<Barber>> Candidates(Service service, Guid? barberId, CancellationToken cancellationToken)
    {
        IReadOnlyList<Barber> active = await barbers.ListAsync(true, cancellationToken);
        return [.. active.Where(barber => barber.Offers(service.Id) && (barberId is null || barber.Id == barberId))];
    }
}
