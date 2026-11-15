using System.Net;
using System.Net.Http.Headers;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas del cambio de contraseña y del acceso de los barberos.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class PasswordTests(ApiFactory factory)
{
    private const string FirstPassword = "barbero-seguro";
    private const string NewPassword = "nueva-contrasena";
    private const string PasswordPath = "/api/auth/password";
    private const string AgendaPath = "/api/agenda?date=2026-11-17";
    private const string BearerScheme = "Bearer";

    /// <summary>Con la contraseña actual correcta se cambia, llega un pase nuevo y el viejo deja de valer.</summary>
    [Fact]
    public async Task ChangePassword_RightCurrent_RenewsSessionAndClosesOldOne()
    {
        string email = NewEmail();
        await Scenario.CreateAsync(factory, email, FirstPassword);
        HttpClient old = await factory.CreateClientAsAsync(email, FirstPassword);

        HttpResponseMessage response = await old.PostJsonAsync(PasswordPath, new ChangePasswordRequest(FirstPassword, NewPassword));
        LoginResponse renewed = await response.ReadAsync<LoginResponse>();
        HttpClient fresh = factory.CreateClient();
        fresh.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BearerScheme, renewed.Token);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await old.GetAsync(AgendaPath, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await fresh.GetAsync(AgendaPath, TestContext.Current.CancellationToken)).StatusCode);
        await factory.CreateClientAsAsync(email, NewPassword);
    }

    /// <summary>Sin la contraseña actual correcta no se cambia nada.</summary>
    [Fact]
    public async Task ChangePassword_WrongCurrent_IsRejected()
    {
        string email = NewEmail();
        await Scenario.CreateAsync(factory, email, FirstPassword);
        HttpClient barber = await factory.CreateClientAsAsync(email, FirstPassword);

        HttpResponseMessage response = await barber.PostJsonAsync(PasswordPath, new ChangePasswordRequest("otra-cosa", NewPassword));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("access.wrong_current_password", await response.ReadErrorCodeAsync());
    }

    /// <summary>El dueño le da acceso a un barbero sin cuenta y luego le pone otra contraseña.</summary>
    [Fact]
    public async Task BarberAccess_CreateThenReset_BarberEntersWithLatest()
    {
        string email = NewEmail();
        Scenario scenario = await Scenario.CreateAsync(factory);
        HttpClient owner = await factory.CreateOwnerClientAsync();
        string accessPath = $"/api/barbers/{scenario.BarberId}/access";

        HttpResponseMessage created = await owner.PutJsonAsync(accessPath, new BarberAccessRequest(email, FirstPassword));
        HttpResponseMessage reset = await owner.PutJsonAsync(accessPath, new BarberAccessRequest(null, NewPassword));
        IReadOnlyList<BarberAccessResponse> list = await (await owner.GetAsync("/api/barbers/access", TestContext.Current.CancellationToken))
            .ReadAsync<IReadOnlyList<BarberAccessResponse>>();

        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        Assert.Contains(list, item => item.BarberId == scenario.BarberId && item.Email == email);
        await factory.CreateClientAsAsync(email, NewPassword);
    }

    /// <summary>Correo nuevo para no chocar con otras pruebas.</summary>
    private static string NewEmail()
    {
        return $"clave-{Guid.NewGuid():N}@prueba.bo";
    }
}
