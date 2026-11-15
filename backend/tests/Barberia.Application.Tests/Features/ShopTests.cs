using Barberia.Application.Features;
using Barberia.Application.Features.Catalog;
using Barberia.Application.Features.Shops;
using Barberia.Application.Tests.Fakes;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;

namespace Barberia.Application.Tests.Features;

/// <summary>Pruebas de los datos, horarios y servicios de la barbería con puertos falsos.</summary>
public sealed class ShopTests
{
    private const string NewName = "Barbería El Prado";
    private const int MinLead = 30;
    private const int MaxDays = 7;
    private const int CancelLimit = 60;
    private const int InvalidMaxDays = 0;
    private const int ServiceMinutes = 15;
    private const int ServiceCents = 1500;
    private const string Holiday = "Feriado";
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Six = new(18, 0);
    private static readonly DateOnly Christmas = new(2026, 12, 25);

    /// <summary>Sin barbería configurada, leerla da error en vez de una barbería vacía.</summary>
    [Fact]
    public async Task GetShop_NotConfigured_Fails()
    {
        Result<Shop> result = await new GetShopHandler(new InMemoryStore()).HandleAsync(TestContext.Current.CancellationToken);

        Assert.Equal(ShopErrors.NotConfigured, result.Error);
    }

    /// <summary>Los datos, las reglas y la paleta se guardan juntos; unas reglas inválidas no cambian nada.</summary>
    [Fact]
    public async Task UpdateShop_SavesAllOrNothing()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        UpdateShopHandler handler = new(world.Store, world.Store);

        Result<Shop> saved = await handler.HandleAsync(new UpdateShopCommand(NewName, "", "", "", MinLead, MaxDays, CancelLimit, ShopPalette.Wine), TestContext.Current.CancellationToken);
        Result<Shop> invalid = await handler.HandleAsync(new UpdateShopCommand("Otro", "", "", "", MinLead, InvalidMaxDays, CancelLimit, ShopPalette.Classic), TestContext.Current.CancellationToken);

        Assert.Equal(ShopPalette.Wine, saved.Value.Palette);
        Assert.True(invalid.IsFailure);
        Assert.Equal(NewName, world.Store.Shop!.Name);
    }

    /// <summary>El horario semanal se reemplaza entero.</summary>
    [Fact]
    public async Task SetOpeningHours_ReplacesWeek()
    {
        DemoWorld world = await DemoWorld.CreateAsync();

        Result<Shop> result = await new SetOpeningHoursHandler(world.Store, world.Store)
            .HandleAsync(new SetOpeningHoursCommand([new HoursInput(DayOfWeek.Monday, Nine, Six)]), TestContext.Current.CancellationToken);

        Assert.Equal(DayOfWeek.Monday, Assert.Single(result.Value.OpeningHours).Day);
    }

    /// <summary>Un día cerrado se agrega una sola vez y se puede quitar.</summary>
    [Fact]
    public async Task ClosedDays_AddTwiceThenRemove()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        AddClosedDayHandler add = new(world.Store, world.Store);

        await add.HandleAsync(new AddClosedDayCommand(Christmas, Holiday), TestContext.Current.CancellationToken);
        Result<Shop> twice = await add.HandleAsync(new AddClosedDayCommand(Christmas, Holiday), TestContext.Current.CancellationToken);
        Result<Shop> removed = await new RemoveClosedDayHandler(world.Store, world.Store).HandleAsync(Christmas, TestContext.Current.CancellationToken);

        Assert.Equal(ShopErrors.ClosedDayExists, twice.Error);
        Assert.Empty(removed.Value.ClosedDays);
    }

    /// <summary>Un servicio nuevo se crea, se oculta y deja de aparecer en la lista pública.</summary>
    [Fact]
    public async Task Services_CreateHideAndList()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        SaveServiceCommand visible = new("Cejas", ServiceMinutes, ServiceCents, true);

        Service created = (await new CreateServiceHandler(world.Store, world.Store).HandleAsync(visible, TestContext.Current.CancellationToken)).Value;
        await new UpdateServiceHandler(world.Store, world.Store).HandleAsync(created.Id, visible with { IsActive = false }, TestContext.Current.CancellationToken);
        IReadOnlyList<Service> publicList = await new ListServicesHandler(world.Store).HandleAsync(true, TestContext.Current.CancellationToken);
        Result<Service> missing = await new UpdateServiceHandler(world.Store, world.Store).HandleAsync(Guid.NewGuid(), visible, TestContext.Current.CancellationToken);

        Assert.DoesNotContain(publicList, service => service.Id == created.Id);
        Assert.Equal(ServiceErrors.NotFound, missing.Error);
    }
}
