using System.Net;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas del catálogo de servicios.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class CatalogEndpointsTests(ApiFactory factory)
{
    private const string ServicesPath = "/api/services";
    private const string InvalidDurationCode = "service.duration";
    private const int FortyFiveMinutes = 45;
    private const int FiftyBolivianos = 5000;
    private const int FifteenMinutes = 15;
    private const int FifteenBolivianos = 1500;
    private const int InvalidMinutes = 7;

    /// <summary>El dueño crea un servicio y el cliente lo ve en la lista pública.</summary>
    [Fact]
    public async Task CreateService_Owner_AppearsInPublicList()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        SaveServiceRequest request = new("Degradado clásico", FortyFiveMinutes, FiftyBolivianos);

        HttpResponseMessage created = await owner.PostJsonAsync(ServicesPath, request);
        ServiceResponse service = await created.ReadAsync<ServiceResponse>();
        List<ServiceResponse> publicList = await (await factory.CreateClient().GetAsync(ServicesPath, TestContext.Current.CancellationToken)).ReadAsync<List<ServiceResponse>>();

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Contains(publicList, item => item.Id == service.Id);
    }

    /// <summary>Un servicio oculto desaparece de la lista pública pero sigue en la del panel.</summary>
    [Fact]
    public async Task UpdateService_Hidden_OnlyInPanelList()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        ServiceResponse service = await (await owner.PostJsonAsync(ServicesPath, new SaveServiceRequest("Cejas", FifteenMinutes, FifteenBolivianos))).ReadAsync<ServiceResponse>();

        await owner.PutJsonAsync($"{ServicesPath}/{service.Id}", new SaveServiceRequest("Cejas", FifteenMinutes, FifteenBolivianos, false));
        List<ServiceResponse> publicList = await (await factory.CreateClient().GetAsync(ServicesPath, TestContext.Current.CancellationToken)).ReadAsync<List<ServiceResponse>>();
        List<ServiceResponse> panelList = await (await owner.GetAsync($"{ServicesPath}/all", TestContext.Current.CancellationToken)).ReadAsync<List<ServiceResponse>>();

        Assert.DoesNotContain(publicList, item => item.Id == service.Id);
        Assert.Contains(panelList, item => item.Id == service.Id && !item.IsActive);
    }

    /// <summary>Una duración inválida se rechaza con su código de error.</summary>
    [Fact]
    public async Task CreateService_InvalidDuration_ReturnsBadRequest()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();

        HttpResponseMessage response = await owner.PostJsonAsync(ServicesPath, new SaveServiceRequest("Rápido", InvalidMinutes, FiftyBolivianos));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(InvalidDurationCode, await response.ReadErrorCodeAsync());
    }
}
