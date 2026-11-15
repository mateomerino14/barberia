using System.Net;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas de los datos, el horario y los días cerrados de la barbería.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class ShopEndpointsTests(ApiFactory factory)
{
    private const string ShopPath = "/api/shop";
    private const string ClosedDaysPath = "/api/shop/closed-days";
    private const string ClosedDayExistsCode = "shop.closed_day_exists";
    private const string NewAddress = "Av. 20 de Octubre 2045";
    private const int HalfHour = 30;
    private const int ThreeWeeks = 21;
    private const int OneHour = 60;
    private static readonly DateOnly Holiday = new(2026, 12, 25);
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Six = new(18, 0);

    /// <summary>Cualquiera puede ver la barbería, porque la usa la página de reservas.</summary>
    [Fact]
    public async Task GetShop_Anonymous_ReturnsShop()
    {
        HttpResponseMessage response = await factory.CreateClient().GetAsync(ShopPath, TestContext.Current.CancellationToken);
        ShopResponse shop = await response.ReadAsync<ShopResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrEmpty(shop.Name));
    }

    /// <summary>Sin sesión no se puede cambiar el horario.</summary>
    [Fact]
    public async Task SetHours_Anonymous_ReturnsUnauthorized()
    {
        SetHoursRequest request = new([new HoursDto(DayOfWeek.Monday, Nine, Six)]);

        HttpResponseMessage response = await factory.CreateClient().PutJsonAsync($"{ShopPath}/hours", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>El dueño cambia los datos y las reglas, y quedan guardados.</summary>
    [Fact]
    public async Task UpdateShop_Owner_SavesProfileAndRules()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        BookingRulesDto rules = new(HalfHour, ThreeWeeks, OneHour);
        UpdateShopRequest request = new(ApiFactory.ShopName, NewAddress, "71234567", "Te esperamos", rules);

        HttpResponseMessage response = await owner.PutJsonAsync(ShopPath, request);
        ShopResponse shop = await response.ReadAsync<ShopResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(NewAddress, shop.Address);
        Assert.Equal(rules, shop.Rules);
    }

    /// <summary>Un día cerrado se agrega una vez; repetirlo es un conflicto, y después se puede quitar.</summary>
    [Fact]
    public async Task ClosedDays_AddRepeatRemove_FollowRules()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        AddClosedDayRequest request = new(Holiday, "Navidad");

        HttpResponseMessage added = await owner.PostJsonAsync(ClosedDaysPath, request);
        HttpResponseMessage repeated = await owner.PostJsonAsync(ClosedDaysPath, request);
        HttpResponseMessage removed = await owner.DeleteAsync($"{ClosedDaysPath}/{Holiday:yyyy-MM-dd}", TestContext.Current.CancellationToken);
        ShopResponse shop = await removed.ReadAsync<ShopResponse>();

        Assert.Equal(HttpStatusCode.OK, added.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, repeated.StatusCode);
        Assert.Equal(ClosedDayExistsCode, await repeated.ReadErrorCodeAsync());
        Assert.DoesNotContain(shop.ClosedDays, day => day.Date == Holiday);
    }
}
