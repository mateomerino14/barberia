using System.Net;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas de horas libres y reservas de punta a punta.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class BookingEndpointsTests(ApiFactory factory)
{
    private const string BookingsPath = "/api/bookings";
    private const string SlotTakenCode = "booking.slot_unavailable";
    private const string TooLateCode = "booking.too_late";
    private const string CancelledStatus = "Cancelled";
    private const int SameTimeRequests = 6;
    private static readonly TimeOnly EightThirty = new(8, 30);
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly NineThirty = new(9, 30);
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly TenFortyFive = new(10, 45);
    private static readonly TimeOnly Eleven = new(11, 0);
    private static readonly TimeOnly Six = new(18, 0);
    private static readonly TimeOnly Eight = new(20, 0);
    private static readonly DateOnly Tomorrow = FixedClock.Today.AddDays(1);

    /// <summary>Una reserva saca la hora de la lista de libres para ese barbero.</summary>
    [Fact]
    public async Task Create_ThenAvailability_HourIsGone()
    {
        Scenario setup = await Scenario.CreateAsync(factory);
        DateTimeOffset start = Scenario.Local(Tomorrow, Ten);

        HttpResponseMessage created = await factory.CreateClient().PostJsonAsync(BookingsPath, Request(setup, setup.BarberId, start));
        List<SlotDto> slots = await GetSlotsAsync(setup, setup.BarberId);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.DoesNotContain(slots, slot => slot.Start == start);
        Assert.Contains(slots, slot => slot.Start == Scenario.Local(Tomorrow, TenFortyFive));
    }

    /// <summary>Si varias personas reservan la misma hora a la vez, solo una se la queda.</summary>
    [Fact]
    public async Task Create_SameHourAtOnce_OnlyOneWins()
    {
        Scenario setup = await Scenario.CreateAsync(factory);
        DateTimeOffset start = Scenario.Local(Tomorrow, Eleven);
        CreateBookingRequest request = Request(setup, setup.BarberId, start);

        HttpResponseMessage[] responses = await Task.WhenAll(Enumerable.Range(0, SameTimeRequests)
            .Select(_ => factory.CreateClient().PostJsonAsync(BookingsPath, request)));

        Assert.Single(responses, response => response.StatusCode == HttpStatusCode.Created);
        Assert.All(responses.Where(response => response.StatusCode != HttpStatusCode.Created), response => Assert.Equal(HttpStatusCode.Conflict, response.StatusCode));
    }

    /// <summary>Con "cualquiera", la reserva va a un barbero libre que hace el servicio.</summary>
    [Fact]
    public async Task Create_AnyBarber_AssignsOneThatOffersTheService()
    {
        Scenario setup = await Scenario.CreateAsync(factory);

        HttpResponseMessage created = await factory.CreateClient().PostJsonAsync(BookingsPath, Request(setup, null, Scenario.Local(Tomorrow, Nine)));
        BookingConfirmation booking = await created.ReadAsync<BookingConfirmation>();

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(setup.BarberId, booking.BarberId);
    }

    /// <summary>El cliente ve su turno con el enlace, lo mueve y lo cancela; la hora vuelve a quedar libre.</summary>
    [Fact]
    public async Task ManageWithToken_RescheduleAndCancel_FreesTheHour()
    {
        Scenario setup = await Scenario.CreateAsync(factory);
        HttpClient client = factory.CreateClient();
        BookingConfirmation booking = await (await client.PostJsonAsync(BookingsPath, Request(setup, setup.BarberId, Scenario.Local(Tomorrow, Six)))).ReadAsync<BookingConfirmation>();
        DateTimeOffset newStart = Scenario.Local(Tomorrow, Six).AddMinutes(-Scenario.ServiceMinutes);

        HttpResponseMessage moved = await client.PutJsonAsync($"{BookingsPath}/{booking.Token}", new RescheduleBookingRequest(newStart, null));
        CustomerBookingResponse afterMove = await moved.ReadAsync<CustomerBookingResponse>();
        HttpResponseMessage cancelled = await client.PostAsync($"{BookingsPath}/{booking.Token}/cancel", null, TestContext.Current.CancellationToken);
        List<SlotDto> slots = await GetSlotsAsync(setup, setup.BarberId);

        Assert.Equal(newStart, afterMove.Start);
        Assert.Equal(CancelledStatus, (await cancelled.ReadAsync<CustomerBookingResponse>()).Status);
        Assert.Contains(slots, slot => slot.Start == newStart);
    }

    /// <summary>Un enlace que no existe no muestra nada.</summary>
    [Fact]
    public async Task Get_UnknownToken_ReturnsNotFound()
    {
        HttpResponseMessage response = await factory.CreateClient().GetAsync($"{BookingsPath}/no-existe", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>Una hora que ya pasó o que no está en el horario se rechaza.</summary>
    [Fact]
    public async Task Create_HourOutsideRules_IsRejected()
    {
        Scenario setup = await Scenario.CreateAsync(factory);

        HttpResponseMessage past = await factory.CreateClient().PostJsonAsync(BookingsPath, Request(setup, setup.BarberId, Scenario.Local(FixedClock.Today, EightThirty)));
        HttpResponseMessage late = await factory.CreateClient().PostJsonAsync(BookingsPath, Request(setup, setup.BarberId, Scenario.Local(Tomorrow, Eight)));

        Assert.Equal(SlotTakenCode, await past.ReadErrorCodeAsync());
        Assert.Equal(SlotTakenCode, await late.ReadErrorCodeAsync());
    }

    /// <summary>Dentro del plazo de cancelación el cliente ya no puede cambiar su turno.</summary>
    [Fact]
    public async Task Cancel_TooCloseToStart_IsRejected()
    {
        Scenario setup = await Scenario.CreateAsync(factory);
        DateTimeOffset soon = Scenario.Local(FixedClock.Today, NineThirty);
        HttpClient client = factory.CreateClient();
        BookingConfirmation booking = await (await client.PostJsonAsync(BookingsPath, Request(setup, setup.BarberId, soon))).ReadAsync<BookingConfirmation>();

        HttpResponseMessage cancelled = await client.PostAsync($"{BookingsPath}/{booking.Token}/cancel", null, TestContext.Current.CancellationToken);

        Assert.Equal(TooLateCode, await cancelled.ReadErrorCodeAsync());
    }

    /// <summary>Pide las horas libres de mañana para el servicio de la prueba.</summary>
    private async Task<List<SlotDto>> GetSlotsAsync(Scenario setup, Guid barberId)
    {
        string path = $"/api/availability?date={Tomorrow:yyyy-MM-dd}&serviceId={setup.ServiceId}&barberId={barberId}";
        return await (await factory.CreateClient().GetAsync(path, TestContext.Current.CancellationToken)).ReadAsync<List<SlotDto>>();
    }

    /// <summary>Arma una reserva de prueba.</summary>
    private static CreateBookingRequest Request(Scenario setup, Guid? barberId, DateTimeOffset start)
    {
        return new CreateBookingRequest(setup.ServiceId, barberId, start, "Diego", "71234567", null);
    }
}
