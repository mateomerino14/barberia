using Barberia.Application.Ports;
using Barberia.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Infrastructure.Persistence.Repositories;

/// <summary>Adaptador de PostgreSQL para el catálogo de servicios.</summary>
public sealed class ServiceRepository(BarberiaDbContext context) : IServiceRepository
{
    /// <summary>Busca un servicio por su Id.</summary>
    public Task<Service?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Services.FirstOrDefaultAsync(service => service.Id == id, cancellationToken);
    }

    /// <summary>Lista los servicios ordenados por nombre.</summary>
    public async Task<IReadOnlyList<Service>> ListAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        IQueryable<Service> query = context.Services;
        if (onlyActive)
        {
            query = query.Where(service => service.IsActive);
        }

        return await query.OrderBy(service => service.Name).ToListAsync(cancellationToken);
    }

    /// <summary>Indica si existen todos los servicios de la lista.</summary>
    public async Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        List<Guid> distinct = [.. ids.Distinct()];
        int found = await context.Services.CountAsync(service => distinct.Contains(service.Id), cancellationToken);
        return found == distinct.Count;
    }

    /// <summary>Agrega un servicio.</summary>
    public void Add(Service service)
    {
        context.Services.Add(service);
    }
}
