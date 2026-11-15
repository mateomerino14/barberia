using Barberia.Api.Contracts;
using Barberia.Api.Http;
using Barberia.Application.Features.Shops;

namespace Barberia.Api.Endpoints;

/// <summary>Rutas de los datos, el horario y los días cerrados de la barbería.</summary>
public static class ShopEndpoints
{
    /// <summary>Registra las rutas de la barbería.</summary>
    public static IEndpointRouteBuilder MapShopEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/shop", GetAsync);
        RouteGroupBuilder owner = routes.MapGroup("/api/shop").RequireAuthorization(Policies.Owner);
        owner.MapPut("/", UpdateAsync);
        owner.MapPut("/hours", SetHoursAsync);
        owner.MapPost("/closed-days", AddClosedDayAsync);
        owner.MapDelete("/closed-days/{date}", RemoveClosedDayAsync);
        return routes;
    }

    /// <summary>Devuelve la barbería; es pública porque la usa la página de reservas.</summary>
    private static async Task<IResult> GetAsync(GetShopHandler handler, CancellationToken cancellationToken)
    {
        return ApiResults.Ok(await handler.HandleAsync(cancellationToken), ShopResponse.From);
    }

    /// <summary>Actualiza los datos y las reglas de reserva.</summary>
    private static async Task<IResult> UpdateAsync(UpdateShopRequest request, UpdateShopHandler handler, CancellationToken cancellationToken)
    {
        UpdateShopCommand command = new(
            request.Name,
            request.Address,
            request.Phone,
            request.WelcomeMessage,
            request.Rules.MinLeadMinutes,
            request.Rules.MaxDaysAhead,
            request.Rules.CancelLimitMinutes,
            request.Palette);
        return ApiResults.Ok(await handler.HandleAsync(command, cancellationToken), ShopResponse.From);
    }

    /// <summary>Reemplaza el horario semanal de atención.</summary>
    private static async Task<IResult> SetHoursAsync(SetHoursRequest request, SetOpeningHoursHandler handler, CancellationToken cancellationToken)
    {
        SetOpeningHoursCommand command = new([.. request.Hours.Select(hours => hours.ToInput())]);
        return ApiResults.Ok(await handler.HandleAsync(command, cancellationToken), ShopResponse.From);
    }

    /// <summary>Marca una fecha como cerrada.</summary>
    private static async Task<IResult> AddClosedDayAsync(AddClosedDayRequest request, AddClosedDayHandler handler, CancellationToken cancellationToken)
    {
        AddClosedDayCommand command = new(request.Date, request.Reason);
        return ApiResults.Ok(await handler.HandleAsync(command, cancellationToken), ShopResponse.From);
    }

    /// <summary>Quita una fecha cerrada.</summary>
    private static async Task<IResult> RemoveClosedDayAsync(DateOnly date, RemoveClosedDayHandler handler, CancellationToken cancellationToken)
    {
        return ApiResults.Ok(await handler.HandleAsync(date, cancellationToken), ShopResponse.From);
    }
}
