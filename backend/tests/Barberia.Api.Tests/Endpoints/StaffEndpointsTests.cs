using System.Net;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas del equipo de barberos y sus permisos.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class StaffEndpointsTests(ApiFactory factory)
{
    private const string BarbersPath = "/api/barbers";
    private const string ServicesPath = "/api/services";
    private const string ShopHoursPath = "/api/shop/hours";
    private const string OutsideShopHoursCode = "barber.outside_shop_hours";
    private const string BarberPassword = "barbero-seguro";
    private const int HalfHour = 30;
    private const int FortyBolivianos = 4000;
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly Six = new(18, 0);
    private static readonly TimeOnly Eight = new(20, 0);
    private static readonly TimeOnly Ten30Night = new(22, 30);
    private static readonly DateOnly VacationStart = new(2026, 12, 1);
    private static readonly DateOnly VacationEnd = new(2026, 12, 5);

    /// <summary>El dueño crea un barbero con cuenta; el barbero entra pero no puede administrar servicios.</summary>
    [Fact]
    public async Task CreateBarber_WithAccount_BarberCanLoginButNotManage()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        string email = $"marco-{Guid.NewGuid():N}@prueba.bo";
        SaveBarberRequest request = new("Marco", [], true, new BarberAccountRequest(email, BarberPassword));

        HttpResponseMessage created = await owner.PostJsonAsync(BarbersPath, request);
        HttpClient barber = await factory.CreateClientAsAsync(email, BarberPassword);
        HttpResponseMessage forbidden = await barber.PostJsonAsync(ServicesPath, new SaveServiceRequest("Corte", HalfHour, FortyBolivianos));
        HttpResponseMessage team = await barber.GetAsync($"{BarbersPath}/all", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);
        Assert.Equal(HttpStatusCode.OK, team.StatusCode);
    }

    /// <summary>El horario del barbero tiene que caber en el de la barbería.</summary>
    [Fact]
    public async Task SetSchedule_InsideAndOutsideShopHours_AcceptsOnlyInside()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        await owner.PutJsonAsync(ShopHoursPath, new SetHoursRequest([new HoursDto(DayOfWeek.Saturday, Nine, Eight)]));
        BarberResponse barber = await (await owner.PostJsonAsync(BarbersPath, new SaveBarberRequest("Rodri", []))).ReadAsync<BarberResponse>();

        HttpResponseMessage inside = await owner.PutJsonAsync($"{BarbersPath}/{barber.Id}/schedule", new SetHoursRequest([new HoursDto(DayOfWeek.Saturday, Ten, Six)]));
        HttpResponseMessage outside = await owner.PutJsonAsync($"{BarbersPath}/{barber.Id}/schedule", new SetHoursRequest([new HoursDto(DayOfWeek.Saturday, Ten, Ten30Night)]));

        Assert.Equal(HttpStatusCode.OK, inside.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, outside.StatusCode);
        Assert.Equal(OutsideShopHoursCode, await outside.ReadErrorCodeAsync());
    }

    /// <summary>Una ausencia se registra y se puede quitar.</summary>
    [Fact]
    public async Task TimeOff_AddAndRemove_UpdatesBarber()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        BarberResponse barber = await (await owner.PostJsonAsync(BarbersPath, new SaveBarberRequest("Lucho", []))).ReadAsync<BarberResponse>();

        BarberResponse withTimeOff = await (await owner.PostJsonAsync($"{BarbersPath}/{barber.Id}/time-off", new AddTimeOffRequest(VacationStart, VacationEnd, "Vacaciones"))).ReadAsync<BarberResponse>();
        TimeOffDto timeOff = Assert.Single(withTimeOff.TimeOff);
        HttpResponseMessage removed = await owner.DeleteAsync($"{BarbersPath}/{barber.Id}/time-off/{timeOff.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, removed.StatusCode);
        Assert.Empty((await removed.ReadAsync<BarberResponse>()).TimeOff);
    }

    /// <summary>Asignar un servicio que no existe se rechaza.</summary>
    [Fact]
    public async Task CreateBarber_UnknownService_ReturnsBadRequest()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();

        HttpResponseMessage response = await owner.PostJsonAsync(BarbersPath, new SaveBarberRequest("Fantasma", [Guid.NewGuid()]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
