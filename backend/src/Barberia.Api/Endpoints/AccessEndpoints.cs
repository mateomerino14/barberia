using System.Security.Claims;
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
        routes.MapPost("/api/auth/login", LoginAsync).RequireRateLimiting(RateLimits.Login);
        routes.MapPost("/api/auth/password", ChangePasswordAsync)
            .RequireAuthorization(Policies.Staff)
            .RequireRateLimiting(RateLimits.Login);
        return routes;
    }

    /// <summary>Abre una sesión con correo y contraseña.</summary>
    private static async Task<IResult> LoginAsync(LoginRequest request, LoginHandler handler, CancellationToken cancellationToken)
    {
        return ApiResults.Ok(
            await handler.HandleAsync(new LoginCommand(request.Email, request.Password), cancellationToken),
            ToResponse);
    }

    /// <summary>Cambia la contraseña propia y devuelve un pase nuevo; las otras sesiones se cierran.</summary>
    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal user,
        ChangePasswordHandler handler,
        CancellationToken cancellationToken)
    {
        ChangePasswordCommand command = new(user.AccountId(), request.CurrentPassword, request.NewPassword);
        return ApiResults.Ok(await handler.HandleAsync(command, cancellationToken), ToResponse);
    }

    /// <summary>Convierte la sesión abierta en la respuesta de la API.</summary>
    private static LoginResponse ToResponse(LoginResult login)
    {
        return new LoginResponse(
            login.Token.Token,
            login.Token.ExpiresAt,
            login.Account.Email,
            login.Account.Role.ToString(),
            login.Account.BarberId);
    }
}
