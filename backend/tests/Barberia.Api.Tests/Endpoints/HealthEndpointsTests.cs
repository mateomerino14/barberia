using System.Net;
using Barberia.Api.Endpoints;
using Barberia.Api.Tests.Support;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas de la ruta de salud de la API.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class HealthEndpointsTests(ApiFactory factory)
{
    private const string HealthPath = "/api/health";
    private const string OnlineStatus = "ok";
    private static readonly TimeSpan BoliviaOffset = TimeSpan.FromHours(-4);

    /// <summary>La ruta de salud responde que la API está en línea con la hora de Bolivia.</summary>
    [Fact]
    public async Task GetHealth_ApiRunning_ReturnsOkWithLocalTime()
    {
        HttpResponseMessage response = await factory.CreateClient().GetAsync(HealthPath, TestContext.Current.CancellationToken);
        HealthResponse body = await response.ReadAsync<HealthResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(OnlineStatus, body.Status);
        Assert.Equal(BoliviaOffset, body.Time.Offset);
    }
}
