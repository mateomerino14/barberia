using Barberia.Application.Features.Bookings;
using Barberia.Application.Ports;
using Barberia.Application.Tests.Fakes;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;

namespace Barberia.Application.Tests.Features;

/// <summary>Pruebas de la reserva del cliente con puertos falsos: horas libres, reservar, ver, cambiar, cancelar y recordar.</summary>
public sealed class BookingTests
{
    private const string CustomerName = "Diego";
    private const string CustomerPhone = "71234567";
    private const string CustomerEmail = "diego@correo.bo";
    private const int RemindHoursBefore = 3;
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly Eleven = new(11, 0);
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Seventeen = new(17, 0);

    /// <summary>Las horas libres empiezan después de la anticipación mínima y desaparecen al reservar.</summary>
    [Fact]
    public async Task Availability_RespectsLeadTimeAndBookings()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        GetAvailabilityHandler availability = new(world.Store, world.Store, world.Store, world.Schedule, world.Clock);
        GetAvailabilityQuery query = new(FakeClock.Today, world.Haircut.Id, world.Lucho.Id);

        IReadOnlyList<FreeSlot> before = (await availability.HandleAsync(query, TestContext.Current.CancellationToken)).Value;
        await BookAsync(world, world.Lucho.Id, Ten);
        IReadOnlyList<FreeSlot> after = (await availability.HandleAsync(query, TestContext.Current.CancellationToken)).Value;

        Assert.DoesNotContain(before, slot => slot.Start < DemoWorld.At(Nine));
        Assert.Contains(before, slot => slot.Start == DemoWorld.At(Ten));
        Assert.DoesNotContain(after, slot => slot.Start == DemoWorld.At(Ten));
    }

    /// <summary>Reservar guarda el turno, lo confirma al cliente y la misma hora ya no se puede tomar.</summary>
    [Fact]
    public async Task Create_ThenSameHour_IsUnavailable()
    {
        DemoWorld world = await DemoWorld.CreateAsync();

        Result<Booking> first = await BookAsync(world, world.Lucho.Id, Ten);
        Result<Booking> second = await BookAsync(world, world.Lucho.Id, Ten);

        Assert.True(first.IsSuccess);
        Assert.Equal(SchedulingErrors.SlotUnavailable, second.Error);
        Assert.Single(world.Notifier.Notices, notice => notice.Event == BookingEvent.Confirmed);
    }

    /// <summary>Con "cualquiera" se elige a un barbero libre.</summary>
    [Fact]
    public async Task Create_AnyBarber_PicksOneFree()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        await BookAsync(world, world.Lucho.Id, Ten);

        Result<Booking> any = await BookAsync(world, null, Ten);

        Assert.True(any.IsSuccess);
        Assert.NotEqual(world.Lucho.Id, any.Value.BarberId);
    }

    /// <summary>El cliente ve su turno con el enlace, lo mueve y lo cancela dentro del plazo.</summary>
    [Fact]
    public async Task ManageByLink_ViewRescheduleCancel()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        Booking booking = (await BookAsync(world, world.Lucho.Id, Eleven)).Value;
        RescheduleBookingHandler reschedule = new(world.Store, world.Store, world.Store, world.Store, world.Schedule, world.Store, world.Clock, world.Notifications);
        CancelBookingHandler cancel = new(world.Store, world.Store, world.Store, world.Clock, world.Notifications);

        Result<BookingDetails> details = await new GetBookingHandler(world.Store, world.Store, world.Store)
            .HandleAsync(booking.ManageToken, TestContext.Current.CancellationToken);
        Result<Booking> moved = await reschedule.HandleAsync(booking.ManageToken, new RescheduleBookingCommand(DemoWorld.At(Seventeen), null), TestContext.Current.CancellationToken);
        Result<Booking> cancelled = await cancel.HandleAsync(booking.ManageToken, TestContext.Current.CancellationToken);

        Assert.Equal(world.Lucho.Name, details.Value.BarberName);
        Assert.Equal(DemoWorld.At(Seventeen), moved.Value.Range.Start);
        Assert.Equal(BookingStatus.Cancelled, cancelled.Value.Status);
    }

    /// <summary>Pasado el plazo, el cliente ya no puede cancelar por su cuenta.</summary>
    [Fact]
    public async Task Cancel_TooLate_IsRejected()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        Booking booking = (await BookAsync(world, world.Lucho.Id, Ten)).Value;
        world.Clock.Now = DemoWorld.At(Nine);

        Result<Booking> result = await new CancelBookingHandler(world.Store, world.Store, world.Store, world.Clock, world.Notifications)
            .HandleAsync(booking.ManageToken, TestContext.Current.CancellationToken);

        Assert.Equal(SchedulingErrors.TooLateToChange, result.Error);
    }

    /// <summary>Un enlace que no existe no muestra nada.</summary>
    [Fact]
    public async Task View_UnknownLink_IsNotFound()
    {
        DemoWorld world = await DemoWorld.CreateAsync();

        Result<BookingDetails> result = await new GetBookingHandler(world.Store, world.Store, world.Store)
            .HandleAsync("no-existe", TestContext.Current.CancellationToken);

        Assert.Equal(SchedulingErrors.NotFound, result.Error);
    }

    /// <summary>El recordatorio sale una sola vez y solo para los turnos cercanos.</summary>
    [Fact]
    public async Task Reminders_OnlyNearAndOnce()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        await BookAsync(world, world.Lucho.Id, Ten);
        await BookAsync(world, world.Lucho.Id, Seventeen);
        SendRemindersHandler reminders = new(world.Store, world.Notifications, world.Clock, world.Store);

        int first = await reminders.HandleAsync(RemindHoursBefore, TestContext.Current.CancellationToken);
        int second = await reminders.HandleAsync(RemindHoursBefore, TestContext.Current.CancellationToken);

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Single(world.Notifier.Notices, notice => notice.Event == BookingEvent.Reminder);
    }

    /// <summary>Reserva como lo haría un cliente desde la página.</summary>
    private static Task<Result<Booking>> BookAsync(DemoWorld world, Guid? barberId, TimeOnly time)
    {
        CreateBookingHandler handler = new(world.Store, world.Store, world.Store, world.Store, world.Schedule, world.Store, world.Clock, world.Notifications);
        CreateBookingCommand command = new(world.Haircut.Id, barberId, DemoWorld.At(time), CustomerName, CustomerPhone, CustomerEmail);
        return handler.HandleAsync(command, TestContext.Current.CancellationToken);
    }
}
