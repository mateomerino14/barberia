using System.Threading.RateLimiting;

namespace Barberia.Api.Http;

/// <summary>Límites de peticiones por dirección IP para las rutas públicas.</summary>
public static class RateLimits
{
    /// <summary>Política para reservar, cambiar y cancelar turnos.</summary>
    public const string PublicWrites = "public-writes";

    /// <summary>Política más estricta para probar contraseñas.</summary>
    public const string Login = "login";

    /// <summary>Política amplia para las consultas públicas: horas libres y turno por enlace.</summary>
    public const string PublicReads = "public-reads";

    private const int DefaultWritesPerMinute = 20;
    private const int DefaultLoginsPerMinute = 5;
    private const int DefaultReadsPerMinute = 120;
    private const string UnknownClient = "desconocido";

    /// <summary>Registra los límites; cada uno se puede ajustar en la sección RateLimits.</summary>
    public static IServiceCollection AddPublicRateLimits(this IServiceCollection services, IConfiguration configuration)
    {
        int writes = configuration.GetValue("RateLimits:PermitsPerMinute", DefaultWritesPerMinute);
        int logins = configuration.GetValue("RateLimits:LoginsPerMinute", DefaultLoginsPerMinute);
        int reads = configuration.GetValue("RateLimits:ReadsPerMinute", DefaultReadsPerMinute);
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(PublicWrites, context => PerClient(context, writes));
            options.AddPolicy(Login, context => PerClient(context, logins));
            options.AddPolicy(PublicReads, context => PerClient(context, reads));
        });
        return services;
    }

    /// <summary>Ventana de un minuto por dirección IP del cliente.</summary>
    private static RateLimitPartition<string> PerClient(HttpContext context, int permits)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? UnknownClient,
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = TimeSpan.FromMinutes(1) });
    }
}
