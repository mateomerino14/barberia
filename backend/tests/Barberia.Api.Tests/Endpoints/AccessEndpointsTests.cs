using System.Net;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas del inicio de sesión del panel.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class AccessEndpointsTests(ApiFactory factory)
{
    private const string LoginPath = "/api/auth/login";
    private const string OwnerRole = "Owner";
    private const string InvalidCredentialsCode = "access.invalid_credentials";

    /// <summary>El dueño creado al arrancar entra con sus credenciales, sin importar mayúsculas en el correo.</summary>
    [Fact]
    public async Task Login_OwnerCredentials_ReturnsToken()
    {
        LoginRequest request = new(ApiFactory.OwnerEmail.ToUpperInvariant(), ApiFactory.OwnerPassword);

        HttpResponseMessage response = await factory.CreateClient().PostJsonAsync(LoginPath, request);
        LoginResponse login = await response.ReadAsync<LoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(OwnerRole, login.Role);
        Assert.False(string.IsNullOrEmpty(login.Token));
    }

    /// <summary>Una contraseña equivocada no abre sesión y no dice si el correo existe.</summary>
    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        LoginRequest request = new(ApiFactory.OwnerEmail, "otra-contrasena");

        HttpResponseMessage response = await factory.CreateClient().PostJsonAsync(LoginPath, request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(InvalidCredentialsCode, await response.ReadErrorCodeAsync());
    }
}
