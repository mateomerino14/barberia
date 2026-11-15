using Barberia.Api.Contracts;
using Barberia.Api.Http;
using Barberia.Application.Features.Access;

namespace Barberia.Api.Endpoints;

/// <summary>Rutas de inicio de sesión del panel.</summary>
public static class AccessEndpoints
{
    /// <summary>Registra la ruta de inicio de sesión.</summary>
    public static IEndpointRouteBuilder MapAccessEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/auth/login", LoginAsync).RequireRateLimiting(RateLimits.PublicWrites);
        return routes;
    }

    /// <summary>Abre una sesión con correo y contraseña.</summary>
    private static async Task<IResult> LoginAsync(LoginRequest request, LoginHandler handler, CancellationToken cancellationToken)
    {
        return ApiResults.Ok(
            await handler.HandleAsync(new LoginCommand(request.Email, request.Password), cancellationToken),
            login => new LoginResponse(
                login.Token.Token,
                login.Token.ExpiresAt,
                login.Account.Email,
                login.Account.Role.ToString(),
                login.Account.BarberId));
    }
}
