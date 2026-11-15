using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Media;
using Barberia.Domain.Scheduling;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Barberia.Infrastructure.Persistence;

/// <summary>Contexto de EF Core sobre PostgreSQL; también confirma los cambios de cada caso de uso.</summary>
public sealed class BarberiaDbContext(DbContextOptions<BarberiaDbContext> options) : DbContext(options), IUnitOfWork
{
    /// <summary>Nombre de la restricción que impide dos turnos activos del mismo barbero a la misma hora.</summary>
    public const string NoOverlapConstraint = "ex_bookings_no_overlap";
    /// <summary>La barbería de esta instalación.</summary>
    public DbSet<Shop> Shops => Set<Shop>();

    /// <summary>Servicios del catálogo.</summary>
    public DbSet<Service> Services => Set<Service>();

    /// <summary>Barberos del equipo.</summary>
    public DbSet<Barber> Barbers => Set<Barber>();

    /// <summary>Cuentas del panel.</summary>
    public DbSet<StaffAccount> StaffAccounts => Set<StaffAccount>();

    /// <summary>Turnos de la agenda.</summary>
    public DbSet<Booking> Bookings => Set<Booking>();

    /// <summary>Bloqueos de la agenda.</summary>
    public DbSet<Block> Blocks => Set<Block>();

    /// <summary>Imágenes subidas por el dueño.</summary>
    public DbSet<StoredImage> Images => Set<StoredImage>();

    /// <summary>Guarda todos los cambios pendientes.</summary>
    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
    }

    /// <summary>Guarda cambios de la agenda y traduce un choque de horario en la base a un conflicto de negocio.</summary>
    public async Task<Result> SaveScheduleChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.ExclusionViolation })
        {
            ChangeTracker.Clear();
            return SchedulingErrors.SlotUnavailable;
        }
    }

    /// <summary>Aplica las configuraciones de cada tabla.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("btree_gist");
        modelBuilder.HasSequence<int>("booking_numbers").StartsAt(1);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BarberiaDbContext).Assembly);
    }
}
