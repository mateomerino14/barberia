using Barberia.Application.Ports;
using Barberia.Domain.Scheduling;

namespace Barberia.Application.Features.Bookings;

/// <summary>Le recuerda el turno a cada cliente unas horas antes, una sola vez.</summary>
public sealed class SendRemindersHandler(IBookingRepository bookings, BookingNotifications notifications, IClock clock, IUnitOfWork unitOfWork)
{
    /// <summary>Avisa los turnos que empiezan dentro de las próximas horas indicadas y devuelve cuántos avisó.</summary>
    public async Task<int> HandleAsync(int hoursBefore, CancellationToken cancellationToken)
    {
        DateTimeOffset now = clock.Now;
        IReadOnlyList<Booking> due = await bookings.ListPendingRemindersAsync(now, now.AddHours(hoursBefore), cancellationToken);
        int sent = 0;
        foreach (Booking booking in due.Where(item => item.NeedsReminder))
        {
            // Se anota antes de avisar: si el correo falla, no se reintenta en cada vuelta.
            booking.MarkReminded(now);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await notifications.SendAsync(BookingEvent.Reminder, booking, cancellationToken);
            sent++;
        }

        return sent;
    }
}
