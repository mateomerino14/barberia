using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;

namespace Barberia.Application.Features.Bookings;

/// <summary>Lee lo que ya ocupa la agenda de un día: turnos activos y bloqueos.</summary>
public sealed class ScheduleReader(IBookingRepository bookings, IBlockRepository blocks)
{
    /// <summary>Devuelve el día completo en hora de La Paz como intervalo en UTC.</summary>
    public static TimeRange DayRange(DateOnly date)
    {
        return TimeRange.Create(BusinessTime.At(date, TimeOnly.MinValue), BusinessTime.At(date.AddDays(1), TimeOnly.MinValue)).Value;
    }

    /// <summary>Devuelve los horarios ocupados de un barbero en un día, sin contar el turno que se está moviendo.</summary>
    public async Task<IReadOnlyList<TimeRange>> BusyAsync(Guid barberId, DateOnly date, Guid? ignoredBookingId, CancellationToken cancellationToken)
    {
        TimeRange day = DayRange(date);
        IReadOnlyList<Booking> dayBookings = await bookings.ListAsync(day, barberId, cancellationToken);
        IReadOnlyList<Block> dayBlocks = await blocks.ListAsync(day, barberId, cancellationToken);
        return
        [
            .. dayBookings.Where(booking => booking.BlocksAgenda && booking.Id != ignoredBookingId).Select(booking => booking.Range),
            .. dayBlocks.Select(block => block.Range),
        ];
    }

    /// <summary>Cuenta los turnos activos de un barbero en un día, para repartir los de "cualquiera".</summary>
    public async Task<int> CountActiveAsync(Guid barberId, DateOnly date, CancellationToken cancellationToken)
    {
        IReadOnlyList<Booking> dayBookings = await bookings.ListAsync(DayRange(date), barberId, cancellationToken);
        return dayBookings.Count(booking => booking.BlocksAgenda);
    }
}
