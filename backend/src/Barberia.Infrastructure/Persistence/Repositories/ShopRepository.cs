using Barberia.Application.Ports;
using Barberia.Domain.Shops;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Infrastructure.Persistence.Repositories;

/// <summary>Adaptador de PostgreSQL para la barbería.</summary>
public sealed class ShopRepository(BarberiaDbContext context) : IShopRepository
{
    /// <summary>Devuelve la única barbería de la instalación.</summary>
    public Task<Shop?> GetAsync(CancellationToken cancellationToken)
    {
        return context.Shops.FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>Agrega la barbería.</summary>
    public void Add(Shop shop)
    {
        context.Shops.Add(shop);
    }
}
