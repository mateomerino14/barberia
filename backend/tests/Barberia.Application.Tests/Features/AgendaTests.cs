using Barberia.Application.Features.Agenda;
using Barberia.Application.Ports;
using Barberia.Application.Tests.Fakes;
using Barberia.Domain.Access;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;

namespace Barberia.Application.Tests.Features;

/// <summary>Pruebas de la agenda del panel con puertos falsos: ver, anotar, marcar, cancelar y bloquear.</summary>
public sealed class AgendaTests
{
    private const string WalkInName = "Juan";
    private const string LunchReason = "Almuerzo";
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly One = new(13, 0);
    private static readonly TimeOnly Two = new(14, 0);
    private static readonly StaffActor Owner = new(StaffRole.Owner, null);

    /// <summary>El dueño ve a todo el equipo; un barbero solo su columna y sus turnos.</summary>
    [Fact]
    public async Task GetAgenda_BarberSeesOnlyOwn()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        await AddWalkInAsync(world, world.Marco.Id, Owner);
        GetAgendaHandler agenda = new(world.Store, world.Store, world.Store, world.Store);

        AgendaDay all = (await agenda.HandleAsync(FakeClock.Today, null, Owner, TestContext.Current.CancellationToken)).Value;
        AgendaDay own = (await agenda.HandleAsync(FakeClock.Today, world.Marco.Id, Barber(world.Lucho.Id), TestContext.Current.CancellationToken)).Value;

        Assert.Equal(world.Store.Barbers.Count, all.Columns.Count);
        Assert.Single(all.Bookings);
        Assert.Equal(world.Lucho.Id, Assert.Single(own.Columns).Barber.Id);
        Assert.Empty(own.Bookings);
    }

    /// <summary>Un barbero no puede anotar turnos en la agenda de otro.</summary>
    [Fact]
    public async Task CreateStaffBooking_OtherBarber_IsForbidden()
    {
        DemoWorld world = await DemoWorld.CreateAsync();

        Result<Booking> result = await AddWalkInAsync(world, world.Marco.Id, Barber(world.Lucho.Id));

        Assert.Equal(StaffActor.OtherBarber, result.Error);
    }

    /// <summary>Marcar atendido se guarda; cancelar desde el panel avisa al cliente.</summary>
    [Fact]
    public async Task SetStatusAndCancel_FromPanel()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        Booking booking = (await AddWalkInAsync(world, world.Lucho.Id, Owner)).Value;
        Booking second = (await AddWalkInAsync(world, world.Marco.Id, Owner)).Value;
        UpdateStaffBookingHandler update = new(world.Store, world.Store, world.Notifications);

        Result<Booking> attended = await update.SetStatusAsync(booking.Id, BookingStatus.Attended, Owner, TestContext.Current.CancellationToken);
        Result<Booking> cancelled = await update.CancelAsync(second.Id, Owner, TestContext.Current.CancellationToken);

        Assert.Equal(BookingStatus.Attended, attended.Value.Status);
        Assert.Equal(BookingStatus.Cancelled, cancelled.Value.Status);
        Assert.Single(world.Notifier.Notices, notice => notice.Event == BookingEvent.Cancelled);
    }

    /// <summary>Un bloqueo se agrega y se quita; uno que no existe da error.</summary>
    [Fact]
    public async Task Block_AddRemoveAndUnknown()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        BlockHandler blocks = new(world.Store, world.Store);

        Result<Block> added = await blocks.AddAsync(new AddBlockCommand(world.Lucho.Id, FakeClock.Today, One, Two, LunchReason), Owner, TestContext.Current.CancellationToken);
        Result removed = await blocks.RemoveAsync(added.Value.Id, Owner, TestContext.Current.CancellationToken);
        Result missing = await blocks.RemoveAsync(Guid.NewGuid(), Owner, TestContext.Current.CancellationToken);

        Assert.True(removed.IsSuccess);
        Assert.Empty(world.Store.Blocks);
        Assert.Equal(SchedulingErrors.BlockNotFound, missing.Error);
    }

    /// <summary>Sesión de un barbero con su ficha.</summary>
    private static StaffActor Barber(Guid barberId)
    {
        return new StaffActor(StaffRole.Barber, barberId);
    }

    /// <summary>Anota a alguien que llegó sin turno.</summary>
    private static Task<Result<Booking>> AddWalkInAsync(DemoWorld world, Guid barberId, StaffActor actor)
    {
        CreateStaffBookingHandler handler = new(world.Store, world.Store, world.Store, world.Store, world.Schedule, world.Store, world.Clock);
        CreateStaffBookingCommand command = new(world.Haircut.Id, barberId, DemoWorld.At(Ten), WalkInName, null, true);
        return handler.HandleAsync(command, actor, TestContext.Current.CancellationToken);
    }
}
