using Barberia.Application.Features.Bookings;
using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Agenda;

/// <summary>Columna de la agenda: un barbero y su horario de ese día.</summary>
public sealed record AgendaColumn(Barber Barber, TimeRange? Workday);

/// <summary>Agenda de un día: barberos, turnos y bloqueos.</summary>
public sealed record AgendaDay(DateOnly Date, IReadOnlyList<AgendaColumn> Columns, IReadOnlyList<Booking> Bookings, IReadOnlyList<Block> Blocks);

/// <summary>Arma la agenda de un día; el barbero solo ve la suya.</summary>
public sealed class GetAgendaHandler(IShopRepository shops, IBarberRepository barbers, IBookingRepository bookings, IBlockRepository blocks)
{
    /// <summary>Devuelve la agenda del día para quien la pide.</summary>
    public async Task<Result<AgendaDay>> HandleAsync(DateOnly date, Guid? barberId, StaffActor actor, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        Guid? scope = actor.Scope(barberId);
        IReadOnlyList<Barber> team = await barbers.ListAsync(true, cancellationToken);
        List<AgendaColumn> columns =
        [
            .. team.Where(barber => scope is null || barber.Id == scope)
                .Select(barber => new AgendaColumn(barber, AvailabilityCalculator.Workday(shop, barber, date))),
        ];
        TimeRange day = ScheduleReader.DayRange(date);
        IReadOnlyList<Booking> dayBookings = await bookings.ListAsync(day, scope, cancellationToken);
        IReadOnlyList<Block> dayBlocks = await blocks.ListAsync(day, scope, cancellationToken);
        return new AgendaDay(date, columns, [.. dayBookings.Where(booking => booking.Status != BookingStatus.Cancelled)], dayBlocks);
    }
}
