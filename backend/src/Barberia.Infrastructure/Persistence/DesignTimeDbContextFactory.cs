using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Barberia.Infrastructure.Persistence;

/// <summary>Arma el contexto para que las herramientas de EF Core generen migraciones sin levantar la API.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BarberiaDbContext>
{
    private const string LocalConnection = "Host=localhost;Database=barberia;Username=barberia;Password=barberia";

    /// <summary>Crea el contexto con la base local de desarrollo.</summary>
    public BarberiaDbContext CreateDbContext(string[] args)
    {
        DbContextOptions<BarberiaDbContext> options = new DbContextOptionsBuilder<BarberiaDbContext>()
            .UseNpgsql(LocalConnection)
            .UseSnakeCaseNamingConvention()
            .Options;
        return new BarberiaDbContext(options);
    }
}
