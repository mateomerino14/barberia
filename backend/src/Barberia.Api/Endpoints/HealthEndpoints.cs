using Barberia.Application.Ports;

namespace Barberia.Api.Endpoints;

/// <summary>Expone el estado del servicio para el monitoreo y el despliegue.</summary>
public static class HealthEndpoints
{
    /// <summary>Registra la ruta de salud de la API.</summary>
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/health", GetHealth);
        return routes;
    }

    /// <summary>Responde que la API está en línea junto con su hora local.</summary>
    private static IResult GetHealth(IClock clock)
    {
        return Results.Ok(new HealthResponse("ok", clock.Now));
    }
}

/// <summary>Respuesta de la ruta de salud.</summary>
public sealed record HealthResponse(string Status, DateTimeOffset Time);
