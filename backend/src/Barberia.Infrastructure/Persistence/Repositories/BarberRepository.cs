using Barberia.Application.Ports;
using Barberia.Domain.Staff;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Infrastructure.Persistence.Repositories;

/// <summary>Adaptador de PostgreSQL para el equipo de barberos.</summary>
public sealed class BarberRepository(BarberiaDbContext context) : IBarberRepository
{
    /// <summary>Busca un barbero por su Id.</summary>
    public Task<Barber?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Barbers.FirstOrDefaultAsync(barber => barber.Id == id, cancellationToken);
    }

    /// <summary>Lista los barberos ordenados por nombre.</summary>
    public async Task<IReadOnlyList<Barber>> ListAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        IQueryable<Barber> query = context.Barbers;
        if (onlyActive)
        {
            query = query.Where(barber => barber.IsActive);
        }

        return await query.OrderBy(barber => barber.Name).ToListAsync(cancellationToken);
    }

    /// <summary>Agrega un barbero.</summary>
    public void Add(Barber barber)
    {
        context.Barbers.Add(barber);
    }
}
