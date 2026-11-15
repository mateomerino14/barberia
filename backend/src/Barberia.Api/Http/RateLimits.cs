using System.Threading.RateLimiting;

namespace Barberia.Api.Http;

/// <summary>Límites de peticiones por dirección IP para las rutas públicas que escriben o prueban contraseñas.</summary>
public static class RateLimits
{
    /// <summary>Política para reservar, cambiar turnos e iniciar sesión.</summary>
    public const string PublicWrites = "public-writes";

    private const int DefaultPermitsPerMinute = 20;
    private const string UnknownClient = "desconocido";

    /// <summary>Registra el límite, configurable con RateLimits:PermitsPerMinute.</summary>
    public static IServiceCollection AddPublicRateLimits(this IServiceCollection services, IConfiguration configuration)
    {
        int permits = configuration.GetValue("RateLimits:PermitsPerMinute", DefaultPermitsPerMinute);
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(PublicWrites, context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? UnknownClient,
                _ => new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = TimeSpan.FromMinutes(1) }));
        });
        return services;
    }
}
