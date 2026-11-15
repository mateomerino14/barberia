using Barberia.Application.Ports;
using Barberia.Domain.Scheduling;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Bookings;

/// <summary>Arma el aviso de un turno con los datos de la barbería y del barbero, y lo envía por todos los canales.</summary>
public sealed class BookingNotifications(IEnumerable<INotifier> notifiers, IShopRepository shops, IBarberRepository barbers)
{
    /// <summary>Avisa al cliente lo que le pasó a su turno.</summary>
    public async Task SendAsync(BookingEvent bookingEvent, Booking booking, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        Barber? barber = await barbers.GetAsync(booking.BarberId, cancellationToken);
        BookingNotice notice = new(bookingEvent, booking, shop?.Name ?? string.Empty, shop?.Address ?? string.Empty, barber?.Name ?? string.Empty);
        foreach (INotifier notifier in notifiers)
        {
            await notifier.NotifyAsync(notice, cancellationToken);
        }
    }
}
