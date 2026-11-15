using Barberia.Domain.Scheduling;

namespace Barberia.Application.Ports;

/// <summary>Qué le pasó al turno, para elegir el mensaje.</summary>
public enum BookingEvent
{
    /// <summary>Se reservó.</summary>
    Confirmed,

    /// <summary>Se cambió la hora.</summary>
    Rescheduled,

    /// <summary>Se canceló.</summary>
    Cancelled,

    /// <summary>Se acerca la hora del turno.</summary>
    Reminder,
}

/// <summary>Aviso al cliente sobre su turno, con lo necesario para escribir el mensaje.</summary>
public sealed record BookingNotice(BookingEvent Event, Booking Booking, string ShopName, string ShopAddress, string BarberName);

/// <summary>Puerto que avisa al cliente por algún canal; un aviso que falla nunca rompe la reserva.</summary>
public interface INotifier
{
    /// <summary>Envía el aviso si el canal está disponible para ese cliente.</summary>
    Task NotifyAsync(BookingNotice notice, CancellationToken cancellationToken);
}
