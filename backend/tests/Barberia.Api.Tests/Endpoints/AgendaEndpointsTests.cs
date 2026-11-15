using System.Net;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;
using Barberia.Application.Ports;
using Barberia.Domain.Scheduling;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas de la agenda del panel, sus permisos y los avisos al cliente.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class AgendaEndpointsTests(ApiFactory factory)
{
    private const string AgendaPath = "/api/agenda";
    private const string BarberPassword = "barbero-seguro";
    private const string AttendedStatus = "Attended";
    private const string CustomerEmail = "diego@correo.bo";
    private static readonly DateOnly Tomorrow = FixedClock.Today.AddDays(1);
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly Noon = new(12, 0);
    private static readonly TimeOnly One = new(13, 0);
    private static readonly TimeOnly TwelveThirty = new(12, 30);

    /// <summary>El dueño ve la agenda con el turno reservado y el horario del barbero.</summary>
    [Fact]
    public async Task GetAgenda_Owner_SeesBookingAndWorkday()
    {
        Scenario scenario = await Scenario.CreateAsync(factory);
        await BookAsync(scenario, Ten, null);
        HttpClient owner = await factory.CreateOwnerClientAsync();

        AgendaResponse agenda = await (await owner.GetAsync(AgendaUrl(scenario.BarberId), TestContext.Current.CancellationToken)).ReadAsync<AgendaResponse>();

        AgendaBarberDto column = Assert.Single(agenda.Barbers);
        Assert.NotNull(column.Opens);
        Assert.Single(agenda.Bookings);
    }

    /// <summary>Un barbero ve solo su agenda y no puede marcar turnos de otro.</summary>
    [Fact]
    public async Task Barber_OnlyOwnAgenda()
    {
        string email = $"agenda-{Guid.NewGuid():N}@prueba.bo";
        Scenario mine = await Scenario.CreateAsync(factory, email, BarberPassword);
        Scenario other = await Scenario.CreateAsync(factory);
        BookingConfirmation othersBooking = await BookAsync(other, Ten, null);
        HttpClient barber = await factory.CreateClientAsAsync(email, BarberPassword);

        AgendaResponse agenda = await (await barber.GetAsync(AgendaUrl(other.BarberId), TestContext.Current.CancellationToken)).ReadAsync<AgendaResponse>();
        HttpClient owner = await factory.CreateOwnerClientAsync();
        AgendaResponse ownerView = await (await owner.GetAsync(AgendaUrl(other.BarberId), TestContext.Current.CancellationToken)).ReadAsync<AgendaResponse>();
        Guid othersId = Assert.Single(ownerView.Bookings, booking => booking.Number == othersBooking.Number).Id;
        HttpResponseMessage forbidden = await barber.PutJsonAsync($"{AgendaPath}/bookings/{othersId}/status", new SetBookingStatusRequest(BookingStatus.Attended));

        Assert.All(agenda.Barbers, column => Assert.Equal(mine.BarberId, column.Id));
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
    }

    /// <summary>El dueño marca un turno como atendido.</summary>
    [Fact]
    public async Task SetStatus_Attended_IsSaved()
    {
        Scenario scenario = await Scenario.CreateAsync(factory);
        await BookAsync(scenario, Ten, null);
        HttpClient owner = await factory.CreateOwnerClientAsync();
        AgendaResponse agenda = await (await owner.GetAsync(AgendaUrl(scenario.BarberId), TestContext.Current.CancellationToken)).ReadAsync<AgendaResponse>();

        HttpResponseMessage response = await owner.PutJsonAsync($"{AgendaPath}/bookings/{agenda.Bookings[0].Id}/status", new SetBookingStatusRequest(BookingStatus.Attended));

        Assert.Equal(AttendedStatus, (await response.ReadAsync<AgendaBookingDto>()).Status);
    }

    /// <summary>Un bloqueo saca esas horas de las libres, y al quitarlo vuelven.</summary>
    [Fact]
    public async Task Block_RemovesAndRestoresHours()
    {
        Scenario scenario = await Scenario.CreateAsync(factory);
        HttpClient owner = await factory.CreateOwnerClientAsync();

        AgendaBlockDto block = await (await owner.PostJsonAsync($"{AgendaPath}/blocks", new AddBlockRequest(scenario.BarberId, Tomorrow, Noon, One, "Almuerzo"))).ReadAsync<AgendaBlockDto>();
        List<SlotDto> blocked = await SlotsAsync(scenario);
        await owner.DeleteAsync($"{AgendaPath}/blocks/{block.Id}", TestContext.Current.CancellationToken);
        List<SlotDto> restored = await SlotsAsync(scenario);

        Assert.DoesNotContain(blocked, slot => slot.Start == Scenario.Local(Tomorrow, TwelveThirty));
        Assert.Contains(restored, slot => slot.Start == Scenario.Local(Tomorrow, TwelveThirty));
    }

    /// <summary>La barbería anota a alguien que llegó sin reserva, sin pedirle celular.</summary>
    [Fact]
    public async Task CreateWalkIn_WithoutPhone_IsSaved()
    {
        Scenario scenario = await Scenario.CreateAsync(factory);
        HttpClient owner = await factory.CreateOwnerClientAsync();
        CreateStaffBookingRequest request = new(scenario.ServiceId, scenario.BarberId, Scenario.Local(Tomorrow, Ten), "Luis", null, true);

        HttpResponseMessage response = await owner.PostJsonAsync($"{AgendaPath}/bookings", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.True((await response.ReadAsync<AgendaBookingDto>()).IsWalkIn);
    }

    /// <summary>Reservar y cancelar desde el panel le avisan al cliente.</summary>
    [Fact]
    public async Task Notifications_OnCreateAndStaffCancel()
    {
        Scenario scenario = await Scenario.CreateAsync(factory);
        BookingConfirmation booking = await BookAsync(scenario, Noon, CustomerEmail);
        HttpClient owner = await factory.CreateOwnerClientAsync();
        AgendaResponse agenda = await (await owner.GetAsync(AgendaUrl(scenario.BarberId), TestContext.Current.CancellationToken)).ReadAsync<AgendaResponse>();

        await owner.PostAsync($"{AgendaPath}/bookings/{agenda.Bookings[0].Id}/cancel", null, TestContext.Current.CancellationToken);

        List<BookingNotice> notices = [.. factory.Notifier.Notices.Where(notice => notice.Booking.Number == booking.Number)];
        Assert.Equal([BookingEvent.Confirmed, BookingEvent.Cancelled], notices.Select(notice => notice.Event));
        Assert.Equal(CustomerEmail, notices[0].Booking.Customer.Email);
    }

    /// <summary>Reserva un turno público de mañana a la hora dada.</summary>
    private async Task<BookingConfirmation> BookAsync(Scenario scenario, TimeOnly time, string? email)
    {
        CreateBookingRequest request = new(scenario.ServiceId, scenario.BarberId, Scenario.Local(Tomorrow, time), "Diego", "71234567", email);
        return await (await factory.CreateClient().PostJsonAsync("/api/bookings", request)).ReadAsync<BookingConfirmation>();
    }

    /// <summary>Pide las horas libres de mañana.</summary>
    private async Task<List<SlotDto>> SlotsAsync(Scenario scenario)
    {
        string path = $"/api/availability?date={Tomorrow:yyyy-MM-dd}&serviceId={scenario.ServiceId}&barberId={scenario.BarberId}";
        return await (await factory.CreateClient().GetAsync(path, TestContext.Current.CancellationToken)).ReadAsync<List<SlotDto>>();
    }

    /// <summary>Ruta de la agenda de mañana.</summary>
    private static string AgendaUrl(Guid barberId)
    {
        return $"{AgendaPath}?date={Tomorrow:yyyy-MM-dd}&barberId={barberId}";
    }
}
