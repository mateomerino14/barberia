using Barberia.Application.Features.Shops;
using Barberia.Application.Tests.Fakes;
using Barberia.Domain.Shops;

namespace Barberia.Application.Tests.Features;

/// <summary>Pruebas de los datos de ejemplo.</summary>
public sealed class SeedDemoTests
{
    private const int ServicesInDemo = 4;
    private const int BarbersInDemo = 3;
    private static readonly DateOnly Saturday = new(2026, 11, 14);
    private static readonly DateOnly Monday = new(2026, 11, 16);

    /// <summary>Carga servicios, barberos con horario y abre la barbería, y no duplica nada si se repite.</summary>
    [Fact]
    public async Task Seed_Twice_LoadsOnce()
    {
        InMemoryStore store = new();
        store.Add(Shop.Create("Barbería").Value);
        SeedDemoHandler handler = new(store, store, store, store);

        await handler.HandleAsync(TestContext.Current.CancellationToken);
        await handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(ServicesInDemo, store.Services.Count);
        Assert.Equal(BarbersInDemo, store.Barbers.Count);
        Assert.All(store.Barbers, barber => Assert.NotNull(barber.HoursOn(Saturday)));
        Assert.Null(store.Shop!.HoursOn(Monday));
    }
}
