using Barberia.Api.Contracts;
using Barberia.Api.Http;
using Barberia.Application.Features.Catalog;
using Barberia.Domain.Catalog;

namespace Barberia.Api.Endpoints;

/// <summary>Rutas del catálogo de servicios.</summary>
public static class CatalogEndpoints
{
    /// <summary>Registra las rutas de servicios.</summary>
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/services", ListActiveAsync);
        RouteGroupBuilder owner = routes.MapGroup("/api/services").RequireAuthorization(Policies.Owner);
        owner.MapGet("/all", ListAllAsync);
        owner.MapPost("/", CreateAsync);
        owner.MapPut("/{id:guid}", UpdateAsync);
        return routes;
    }

    /// <summary>Lista los servicios que el cliente puede reservar.</summary>
    private static async Task<IResult> ListActiveAsync(ListServicesHandler handler, CancellationToken cancellationToken)
    {
        IReadOnlyList<Service> services = await handler.HandleAsync(true, cancellationToken);
        return Results.Ok(services.Select(ServiceResponse.From));
    }

    /// <summary>Lista todos los servicios, también los ocultos.</summary>
    private static async Task<IResult> ListAllAsync(ListServicesHandler handler, CancellationToken cancellationToken)
    {
        IReadOnlyList<Service> services = await handler.HandleAsync(false, cancellationToken);
        return Results.Ok(services.Select(ServiceResponse.From));
    }

    /// <summary>Crea un servicio.</summary>
    private static async Task<IResult> CreateAsync(SaveServiceRequest request, CreateServiceHandler handler, CancellationToken cancellationToken)
    {
        SaveServiceCommand command = new(request.Name, request.DurationMinutes, request.PriceCents, request.IsActive);
        return ApiResults.Created(
            await handler.HandleAsync(command, cancellationToken),
            ServiceResponse.From,
            service => $"/api/services/{service.Id}");
    }

    /// <summary>Edita un servicio.</summary>
    private static async Task<IResult> UpdateAsync(Guid id, SaveServiceRequest request, UpdateServiceHandler handler, CancellationToken cancellationToken)
    {
        SaveServiceCommand command = new(request.Name, request.DurationMinutes, request.PriceCents, request.IsActive);
        return ApiResults.Ok(await handler.HandleAsync(id, command, cancellationToken), ServiceResponse.From);
    }
}
