using Barberia.Application.Features;
using Barberia.Application.Features.Staff;
using Barberia.Application.Tests.Fakes;
using Barberia.Domain.Access;
using Barberia.Domain.Common;
using Barberia.Domain.Staff;

namespace Barberia.Application.Tests.Features;

/// <summary>Pruebas de los casos de uso del equipo.</summary>
public sealed class StaffTests
{
    private const string BarberName = "Marco";
    private const string BarberEmail = "marco@barberia.bo";
    private const string BarberPassword = "barbero-seguro";

    private static readonly TimeOnly Seven = new(7, 0);
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly Six = new(18, 0);
    private static readonly DateOnly VacationStart = new(2026, 12, 1);
    private static readonly DateOnly VacationEnd = new(2026, 12, 7);

    private readonly InMemoryStore _store = new();

    /// <summary>Crear un barbero con cuenta guarda ambos y los confirma juntos.</summary>
    [Fact]
    public async Task CreateBarber_WithAccount_SavesBothOnce()
    {
        CreateBarberHandler handler = new(_store, _store, _store, new FakePasswordHasher(), _store);

        Result<Barber> barber = await handler.HandleAsync(new SaveBarberCommand(BarberName, true, []), new BarberAccountInput(BarberEmail, BarberPassword), TestContext.Current.CancellationToken);

        StaffAccount account = Assert.Single(_store.Accounts);
        Assert.Equal(barber.Value.Id, account.BarberId);
        Assert.Equal(1, _store.Saves);
    }

    /// <summary>Si el correo ya está en uso, no se crea ni el barbero ni la cuenta.</summary>
    [Fact]
    public async Task CreateBarber_EmailTaken_SavesNothing()
    {
        CreateBarberHandler handler = new(_store, _store, _store, new FakePasswordHasher(), _store);
        BarberAccountInput account = new(BarberEmail, BarberPassword);
        await handler.HandleAsync(new SaveBarberCommand(BarberName, true, []), account, TestContext.Current.CancellationToken);

        Result<Barber> second = await handler.HandleAsync(new SaveBarberCommand("Otro", true, []), account, TestContext.Current.CancellationToken);

        Assert.Equal(AccessErrors.EmailTaken, second.Error);
        Assert.Single(_store.Barbers);
    }

    /// <summary>Editar un barbero cambia su nombre y estado; uno que no existe da error.</summary>
    [Fact]
    public async Task UpdateBarber_RenamesAndDeactivates()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        UpdateBarberHandler handler = new(world.Store, world.Store, world.Store);

        Result<Barber> updated = await handler.HandleAsync(world.Lucho.Id, new SaveBarberCommand("Luis", false, []), TestContext.Current.CancellationToken);
        Result<Barber> missing = await handler.HandleAsync(Guid.NewGuid(), new SaveBarberCommand("Nadie", true, []), TestContext.Current.CancellationToken);
        IReadOnlyList<Barber> active = await new ListBarbersHandler(world.Store).HandleAsync(true, TestContext.Current.CancellationToken);

        Assert.Equal("Luis", updated.Value.Name);
        Assert.DoesNotContain(active, barber => barber.Id == world.Lucho.Id);
        Assert.Equal(StaffErrors.NotFound, missing.Error);
    }

    /// <summary>El horario del barbero no puede salirse del horario de la barbería.</summary>
    [Fact]
    public async Task SetSchedule_OutsideShopHours_Fails()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        SetBarberScheduleHandler handler = new(world.Store, world.Store, world.Store);

        Result<Barber> inside = await handler.HandleAsync(world.Lucho.Id, new SetBarberScheduleCommand([new HoursInput(DayOfWeek.Tuesday, Ten, Six)]), TestContext.Current.CancellationToken);
        Result<Barber> outside = await handler.HandleAsync(world.Lucho.Id, new SetBarberScheduleCommand([new HoursInput(DayOfWeek.Tuesday, Seven, Six)]), TestContext.Current.CancellationToken);

        Assert.True(inside.IsSuccess);
        Assert.Equal(StaffErrors.OutsideShopHours, outside.Error);
    }

    /// <summary>Una ausencia se registra y se quita.</summary>
    [Fact]
    public async Task TimeOff_AddThenRemove()
    {
        DemoWorld world = await DemoWorld.CreateAsync();

        Result<Barber> added = await new AddTimeOffHandler(world.Store, world.Store)
            .HandleAsync(world.Lucho.Id, new AddTimeOffCommand(VacationStart, VacationEnd, "Vacaciones"), TestContext.Current.CancellationToken);
        Guid timeOffId = Assert.Single(added.Value.TimeOff).Id;
        Result<Barber> removed = await new RemoveTimeOffHandler(world.Store, world.Store)
            .HandleAsync(world.Lucho.Id, timeOffId, TestContext.Current.CancellationToken);

        Assert.Empty(removed.Value.TimeOff);
    }
}
