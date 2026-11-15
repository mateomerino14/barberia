using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Catalog;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Infrastructure.Persistence;

/// <summary>Contexto de EF Core sobre PostgreSQL; también confirma los cambios de cada caso de uso.</summary>
public sealed class BarberiaDbContext(DbContextOptions<BarberiaDbContext> options) : DbContext(options), IUnitOfWork
{
    /// <summary>La barbería de esta instalación.</summary>
    public DbSet<Shop> Shops => Set<Shop>();

    /// <summary>Servicios del catálogo.</summary>
    public DbSet<Service> Services => Set<Service>();

    /// <summary>Barberos del equipo.</summary>
    public DbSet<Barber> Barbers => Set<Barber>();

    /// <summary>Cuentas del panel.</summary>
    public DbSet<StaffAccount> StaffAccounts => Set<StaffAccount>();

    /// <summary>Guarda todos los cambios pendientes.</summary>
    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
    }

    /// <summary>Aplica las configuraciones de cada tabla.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BarberiaDbContext).Assembly);
    }
}
