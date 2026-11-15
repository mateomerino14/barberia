using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;
using Barberia.Application.Features.Bookings;
using Barberia.Application.Ports;
using Microsoft.Extensions.DependencyInjection;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas de los recordatorios antes del turno.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class ReminderTests(ApiFactory factory)
{
    private const int HoursBefore = 3;
    private static readonly TimeOnly TenThirty = new(10, 30);
    private static readonly TimeOnly Seventeen = new(17, 0);

    /// <summary>Un turno dentro de las próximas horas recibe un solo recordatorio; uno más lejano todavía no.</summary>
    [Fact]
    public async Task SendReminders_OnlyNearBookings_OnlyOnce()
    {
        Scenario setup = await Scenario.CreateAsync(factory);
        BookingConfirmation near = await BookAsync(setup, TenThirty);
        BookingConfirmation far = await BookAsync(setup, Seventeen);

        await RunAsync();
        await RunAsync();

        Assert.Single(factory.Notifier.Notices, notice => notice.Event == BookingEvent.Reminder && notice.Booking.Number == near.Number);
        Assert.DoesNotContain(factory.Notifier.Notices, notice => notice.Event == BookingEvent.Reminder && notice.Booking.Number == far.Number);
    }

    /// <summary>Reserva para hoy a la hora indicada.</summary>
    private async Task<BookingConfirmation> BookAsync(Scenario setup, TimeOnly time)
    {
        CreateBookingRequest request = new(setup.ServiceId, setup.BarberId, Scenario.Local(FixedClock.Today, time), "Diego", "71234567", "diego@prueba.bo");
        return await (await factory.CreateClient().PostJsonAsync("/api/bookings", request)).ReadAsync<BookingConfirmation>();
    }

    /// <summary>Corre una tanda de recordatorios como lo haría la tarea de fondo.</summary>
    private async Task RunAsync()
    {
        using IServiceScope scope = factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<SendRemindersHandler>().HandleAsync(HoursBefore, TestContext.Current.CancellationToken);
    }
}
