using Barberia.Application.Features.Bookings;
using Barberia.Application.Features.Shops;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Staff;

namespace Barberia.Application.Tests.Fakes;

/// <summary>Barbería de demostración en memoria, con reloj y avisos de prueba, para probar los casos de uso.</summary>
public sealed class DemoWorld
{
    private const string ShopName = "Barbería Don Lucho";
    private const string OwnerEmail = "dueno@barberia.bo";
    private const string OwnerPassword = "contrasena-segura";

    /// <summary>Datos guardados.</summary>
    public InMemoryStore Store { get; } = new();

    /// <summary>Reloj de la prueba.</summary>
    public FakeClock Clock { get; } = new();

    /// <summary>Avisos que se habrían enviado.</summary>
    public CapturingNotifier Notifier { get; } = new();

    /// <summary>Contraseñas de prueba.</summary>
    public FakePasswordHasher Hasher { get; } = new();

    /// <summary>Lectura de la agenda sobre los datos en memoria.</summary>
    public ScheduleReader Schedule => new(Store, Store);

    /// <summary>Avisos al cliente sobre los datos en memoria.</summary>
    public BookingNotifications Notifications => new([Notifier], Store, Store);

    /// <summary>Primer barbero del equipo.</summary>
    public Barber Lucho => Store.Barbers[0];

    /// <summary>Segundo barbero del equipo.</summary>
    public Barber Marco => Store.Barbers[1];

    /// <summary>Primer servicio del catálogo.</summary>
    public Service Haircut => Store.Services[0];

    /// <summary>Crea la barbería, su dueño y los datos de demostración.</summary>
    public static async Task<DemoWorld> CreateAsync()
    {
        DemoWorld world = new();
        await new InitializeShopHandler(world.Store, world.Store, world.Hasher, world.Store)
            .HandleAsync(new InitializeShopCommand(ShopName, OwnerEmail, OwnerPassword), TestContext.Current.CancellationToken);
        await new SeedDemoHandler(world.Store, world.Store, world.Store, world.Store).HandleAsync(TestContext.Current.CancellationToken);
        return world;
    }

    /// <summary>Hora de La Paz del día de prueba, con su desfase.</summary>
    public static DateTimeOffset At(TimeOnly time)
    {
        return BusinessTime.ToLocal(BusinessTime.At(FakeClock.Today, time));
    }
}
