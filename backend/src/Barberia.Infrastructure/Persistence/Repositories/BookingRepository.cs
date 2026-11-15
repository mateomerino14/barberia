using Barberia.Application.Ports;
using Barberia.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Infrastructure.Persistence.Repositories;

/// <summary>Adaptador de PostgreSQL para los turnos.</summary>
public sealed class BookingRepository(BarberiaDbContext context) : IBookingRepository
{
    /// <summary>Busca un turno por su Id.</summary>
    public Task<Booking?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Bookings.FirstOrDefaultAsync(booking => booking.Id == id, cancellationToken);
    }

    /// <summary>Busca un turno por la clave de su enlace.</summary>
    public Task<Booking?> FindByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return context.Bookings.FirstOrDefaultAsync(booking => booking.ManageToken == token, cancellationToken);
    }

    /// <summary>Lista los turnos que empiezan dentro del intervalo, ordenados por hora.</summary>
    public async Task<IReadOnlyList<Booking>> ListAsync(TimeRange range, Guid? barberId, CancellationToken cancellationToken)
    {
        IQueryable<Booking> query = context.Bookings.Where(booking => booking.Range.Start >= range.Start && booking.Range.Start < range.End);
        if (barberId is not null)
        {
            query = query.Where(booking => booking.BarberId == barberId);
        }

        return await query.OrderBy(booking => booking.Range.Start).ToListAsync(cancellationToken);
    }

    /// <summary>Lista los turnos reservados que empiezan antes de esa hora y todavía no tienen recordatorio.</summary>
    public async Task<IReadOnlyList<Booking>> ListPendingRemindersAsync(DateTimeOffset from, DateTimeOffset until, CancellationToken cancellationToken)
    {
        DateTimeOffset start = from.ToUniversalTime();
        DateTimeOffset end = until.ToUniversalTime();
        return await context.Bookings
            .Where(booking => booking.Status == BookingStatus.Booked && !booking.IsWalkIn && booking.RemindedAt == null)
            .Where(booking => booking.Range.Start > start && booking.Range.Start <= end)
            .OrderBy(booking => booking.Range.Start)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Agrega un turno.</summary>
    public void Add(Booking booking)
    {
        context.Bookings.Add(booking);
    }
}
