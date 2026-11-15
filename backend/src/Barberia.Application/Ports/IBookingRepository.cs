using Barberia.Domain.Scheduling;

namespace Barberia.Application.Ports;

/// <summary>Puerto de persistencia de los turnos.</summary>
public interface IBookingRepository
{
    /// <summary>Busca un turno por su Id.</summary>
    Task<Booking?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Busca un turno por la clave secreta de su enlace.</summary>
    Task<Booking?> FindByTokenAsync(string token, CancellationToken cancellationToken);

    /// <summary>Lista los turnos que empiezan dentro del intervalo, de un barbero o de todos.</summary>
    Task<IReadOnlyList<Booking>> ListAsync(TimeRange range, Guid? barberId, CancellationToken cancellationToken);

    /// <summary>Lista los turnos reservados que empiezan antes de esa hora y todavía no tienen recordatorio.</summary>
    Task<IReadOnlyList<Booking>> ListPendingRemindersAsync(DateTimeOffset from, DateTimeOffset until, CancellationToken cancellationToken);

    /// <summary>Agrega un turno.</summary>
    void Add(Booking booking);
}
