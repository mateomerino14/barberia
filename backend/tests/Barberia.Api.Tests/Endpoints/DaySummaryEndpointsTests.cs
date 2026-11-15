using System.Net;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas del resumen de días libres y del intervalo de turnos.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class DaySummaryEndpointsTests(ApiFactory factory)
{
    private const int DaysAhead = 14;
    private const int HalfHour = 30;
    private const int QuarterHour = 15;
    private const int MinLead = 60;
    private const int CancelLimit = 120;
    private const int InvalidStep = 25;

    /// <summary>Sin servicio elegido devuelve todos los días que se pueden reservar, desde hoy.</summary>
    [Fact]
    public async Task Days_WithoutService_CoversWindow()
    {
        await Scenario.CreateAsync(factory);

        HttpResponseMessage response = await factory.CreateClient().GetAsync("/api/availability/days", TestContext.Current.CancellationToken);
        IReadOnlyList<DaySummaryDto> days = await response.ReadAsync<IReadOnlyList<DaySummaryDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(DaysAhead + 1, days.Count);
        Assert.Equal(FixedClock.Today, days[0].Date);
        Assert.Contains(days, day => day.Open && day.FreeStarts > 0);
    }

    /// <summary>Con turnos cada 30 minutos, las horas libres van de media en media hora; un intervalo raro no se acepta.</summary>
    [Fact]
    public async Task SlotStep_HalfHour_ChangesAvailability()
    {
        Scenario scenario = await Scenario.CreateAsync(factory);
        HttpClient owner = await factory.CreateOwnerClientAsync();

        HttpResponseMessage invalid = await owner.PutJsonAsync("/api/shop", Profile(InvalidStep));
        await owner.PutJsonAsync("/api/shop", Profile(HalfHour));
        IReadOnlyList<SlotDto> slots = await (await factory.CreateClient().GetAsync(
            $"/api/availability?date={FixedClock.Today.AddDays(1):yyyy-MM-dd}&serviceId={scenario.ServiceId}&barberId={scenario.BarberId}",
            TestContext.Current.CancellationToken)).ReadAsync<IReadOnlyList<SlotDto>>();
        await owner.PutJsonAsync("/api/shop", Profile(QuarterHour));

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.All(slots.Zip(slots.Skip(1)), pair => Assert.Equal(TimeSpan.FromMinutes(HalfHour), pair.Second.Start - pair.First.Start));
    }

    /// <summary>Datos de la barbería con el intervalo pedido.</summary>
    private static UpdateShopRequest Profile(int slotStep)
    {
        return new UpdateShopRequest(ApiFactory.ShopName, "", "", "", new BookingRulesDto(MinLead, DaysAhead, CancelLimit, slotStep));
    }
}
