using System.Net;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas de las cabeceras de seguridad y del cierre de sesiones.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class SecurityTests(ApiFactory factory)
{
    private const string BarberPassword = "barbero-seguro";
    private const string AgendaPath = "/api/agenda?date=2026-11-17";

    /// <summary>Cada respuesta prohíbe incrustarla, adivinar su tipo, guardarla en caché y mandar la dirección de origen.</summary>
    [Fact]
    public async Task AnyResponse_HasSecurityHeaders()
    {
        HttpResponseMessage response = await factory.CreateClient().GetAsync("/api/health", TestContext.Current.CancellationToken);

        Assert.Equal("nosniff", Header(response, "X-Content-Type-Options"));
        Assert.Equal("DENY", Header(response, "X-Frame-Options"));
        Assert.Equal("no-referrer", Header(response, "Referrer-Policy"));
        Assert.Equal("no-store", Header(response, "Cache-Control"));
    }

    /// <summary>Al dar de baja a un barbero, su sesión abierta deja de funcionar al instante.</summary>
    [Fact]
    public async Task DeactivatedBarber_OpenSession_IsRejected()
    {
        string email = $"baja-{Guid.NewGuid():N}@prueba.bo";
        Scenario scenario = await Scenario.CreateAsync(factory, email, BarberPassword);
        HttpClient barber = await factory.CreateClientAsAsync(email, BarberPassword);
        HttpClient owner = await factory.CreateOwnerClientAsync();

        HttpResponseMessage before = await barber.GetAsync(AgendaPath, TestContext.Current.CancellationToken);
        await owner.PutJsonAsync($"/api/barbers/{scenario.BarberId}", new SaveBarberRequest("Barbero", [scenario.ServiceId], false));
        HttpResponseMessage after = await barber.GetAsync(AgendaPath, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, before.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
    }

    /// <summary>Lee el valor de una cabecera de la respuesta o de su contenido.</summary>
    private static string? Header(HttpResponseMessage response, string name)
    {
        if (response.Headers.TryGetValues(name, out IEnumerable<string>? values))
        {
            return string.Join(",", values);
        }

        if (response.Content.Headers.TryGetValues(name, out IEnumerable<string>? contentValues))
        {
            return string.Join(",", contentValues);
        }

        return null;
    }
}
