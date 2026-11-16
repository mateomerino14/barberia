using System.Security.Claims;
using Barberia.Api.Contracts;
using Barberia.Api.Http;
using Barberia.Application.Features.Access;
using Barberia.Domain.Common;

namespace Barberia.Api.Endpoints;

/// <summary>Rutas de inicio de sesión del panel.</summary>
public static class AccessEndpoints
{
    /// <summary>Registra la ruta de inicio de sesión.</summary>
    public static IEndpointRouteBuilder MapAccessEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/auth/login", LoginAsync).RequireRateLimiting(RateLimits.Login);
        routes.MapGet("/api/auth/password-reset", GetResetStatus);
        routes.MapPost("/api/auth/password-reset", RequestResetAsync).RequireRateLimiting(RateLimits.Login);
        routes.MapPost("/api/auth/password-reset/confirm", ConfirmResetAsync).RequireRateLimiting(RateLimits.Login);
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

    /// <summary>Dice si la web debe ofrecer "¿Olvidaste tu contraseña?".</summary>
    private static IResult GetResetStatus(PasswordResetStatusHandler handler)
    {
        return Results.Ok(new PasswordResetStatusResponse(handler.Handle()));
    }

    /// <summary>Pide el enlace por correo; responde siempre lo mismo para no revelar qué correos tienen cuenta.</summary>
    private static async Task<IResult> RequestResetAsync(PasswordResetRequest request, RequestPasswordResetHandler handler, CancellationToken cancellationToken)
    {
        await handler.HandleAsync(request.Email, cancellationToken);
        return Results.Accepted();
    }

    /// <summary>Pone la contraseña nueva con la clave del enlace.</summary>
    private static async Task<IResult> ConfirmResetAsync(PasswordResetConfirmRequest request, ResetPasswordHandler handler, CancellationToken cancellationToken)
    {
        Result result = await handler.HandleAsync(new ResetPasswordCommand(request.Token, request.Password), cancellationToken);
        if (result.IsFailure)
        {
            return ApiResults.Problem(result.Error!);
        }

        return Results.NoContent();
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
