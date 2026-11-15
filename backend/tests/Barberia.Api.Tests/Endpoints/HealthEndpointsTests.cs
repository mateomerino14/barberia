using System.Net;
using System.Net.Http.Json;
using Barberia.Api.Endpoints;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas de la ruta de salud de la API.</summary>
public sealed class HealthEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private const string HealthPath = "/api/health";
    private const string OnlineStatus = "ok";
    private static readonly TimeSpan BoliviaOffset = TimeSpan.FromHours(-4);

    /// <summary>La ruta de salud responde que la API está en línea con la hora de Bolivia.</summary>
    [Fact]
    public async Task GetHealth_ApiRunning_ReturnsOkWithLocalTime()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync(HealthPath, TestContext.Current.CancellationToken);
        HealthResponse? body = await response.Content.ReadFromJsonAsync<HealthResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(OnlineStatus, body.Status);
        Assert.Equal(BoliviaOffset, body.Time.Offset);
    }
}
