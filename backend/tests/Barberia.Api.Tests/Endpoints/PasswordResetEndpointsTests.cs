using System.Net;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;
using Barberia.Application.Ports;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas de recuperar la contraseña por correo de punta a punta en la API.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class PasswordResetEndpointsTests(ApiFactory factory)
{
    private const string FirstPassword = "barbero-seguro";
    private const string NewPassword = "nueva-contrasena";
    private const string ResetPath = "/api/auth/password-reset";
    private const string ConfirmPath = "/api/auth/password-reset/confirm";
    private const string AgendaPath = "/api/agenda?date=2026-11-17";

    /// <summary>La web sabe que puede ofrecer "¿Olvidaste tu contraseña?".</summary>
    [Fact]
    public async Task Status_WithEmail_IsAvailable()
    {
        PasswordResetStatusResponse status = await (await factory.CreateClient().GetAsync(ResetPath, TestContext.Current.CancellationToken))
            .ReadAsync<PasswordResetStatusResponse>();

        Assert.True(status.Available);
    }

    /// <summary>Pedir el enlace, usarlo y entrar con la contraseña nueva; la sesión vieja se cierra y el enlace no sirve dos veces.</summary>
    [Fact]
    public async Task RequestAndConfirm_ChangesPasswordAndClosesSessions()
    {
        string email = $"olvido-{Guid.NewGuid():N}@prueba.bo";
        await Scenario.CreateAsync(factory, email, FirstPassword);
        HttpClient oldSession = await factory.CreateClientAsAsync(email, FirstPassword);
        HttpClient anonymous = factory.CreateClient();

        HttpResponseMessage requested = await anonymous.PostJsonAsync(ResetPath, new PasswordResetRequest(email));
        PasswordResetNotice notice = factory.AccessNotifier.Sent.Last(item => item.Email == email);
        HttpResponseMessage confirmed = await anonymous.PostJsonAsync(ConfirmPath, new PasswordResetConfirmRequest(notice.Token, NewPassword));
        HttpResponseMessage reused = await anonymous.PostJsonAsync(ConfirmPath, new PasswordResetConfirmRequest(notice.Token, NewPassword));

        Assert.Equal(HttpStatusCode.Accepted, requested.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, confirmed.StatusCode);
        Assert.Equal("access.reset_link_invalid", await reused.ReadErrorCodeAsync());
        Assert.Equal(HttpStatusCode.Unauthorized, (await oldSession.GetAsync(AgendaPath, TestContext.Current.CancellationToken)).StatusCode);
        await factory.CreateClientAsAsync(email, NewPassword);
    }

    /// <summary>Un correo sin cuenta recibe la misma respuesta y no se manda nada.</summary>
    [Fact]
    public async Task Request_UnknownEmail_SameAnswerNoEmail()
    {
        string email = $"nadie-{Guid.NewGuid():N}@prueba.bo";

        HttpResponseMessage response = await factory.CreateClient().PostJsonAsync(ResetPath, new PasswordResetRequest(email));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.DoesNotContain(factory.AccessNotifier.Sent, item => item.Email == email);
    }
}
