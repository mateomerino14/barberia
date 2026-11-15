using System.Security.Claims;
using Barberia.Api.Contracts;
using Barberia.Api.Http;
using Barberia.Application.Features.Agenda;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;

namespace Barberia.Api.Endpoints;

/// <summary>Rutas de la agenda del panel: ver el día, anotar turnos, marcarlos y bloquear horas.</summary>
public static class AgendaEndpoints
{
    /// <summary>Registra las rutas de la agenda.</summary>
    public static IEndpointRouteBuilder MapAgendaEndpoints(this IEndpointRouteBuilder routes)
    {
        RouteGroupBuilder staff = routes.MapGroup("/api/agenda").RequireAuthorization(Policies.Staff);
        staff.MapGet("/", GetAsync);
        staff.MapPost("/bookings", CreateBookingAsync);
        staff.MapPut("/bookings/{id:guid}/status", SetStatusAsync);
        staff.MapPost("/bookings/{id:guid}/cancel", CancelAsync);
        staff.MapPost("/blocks", AddBlockAsync);
        staff.MapDelete("/blocks/{id:guid}", RemoveBlockAsync);
        return routes;
    }

    /// <summary>Devuelve la agenda de un día.</summary>
    private static async Task<IResult> GetAsync(DateOnly date, Guid? barberId, ClaimsPrincipal user, GetAgendaHandler handler, CancellationToken cancellationToken)
    {
        return ApiResults.Ok(await handler.HandleAsync(date, barberId, user.ToActor(), cancellationToken), AgendaResponse.From);
    }

    /// <summary>Anota un turno desde el panel.</summary>
    private static async Task<IResult> CreateBookingAsync(
        CreateStaffBookingRequest request,
        ClaimsPrincipal user,
        CreateStaffBookingHandler handler,
        CancellationToken cancellationToken)
    {
        CreateStaffBookingCommand command = new(
            request.ServiceId,
            request.BarberId,
            request.Start,
            request.CustomerName,
            request.CustomerPhone,
            request.IsWalkIn);
        return ApiResults.Created(
            await handler.HandleAsync(command, user.ToActor(), cancellationToken),
            AgendaBookingDto.From,
            booking => $"/api/agenda/bookings/{booking.Id}");
    }

    /// <summary>Marca cómo terminó un turno.</summary>
    private static async Task<IResult> SetStatusAsync(
        Guid id,
        SetBookingStatusRequest request,
        ClaimsPrincipal user,
        UpdateStaffBookingHandler handler,
        CancellationToken cancellationToken)
    {
        Result<Booking> result = await handler.SetStatusAsync(id, request.Status, user.ToActor(), cancellationToken);
        return ApiResults.Ok(result, AgendaBookingDto.From);
    }

    /// <summary>Cancela un turno desde el panel.</summary>
    private static async Task<IResult> CancelAsync(Guid id, ClaimsPrincipal user, UpdateStaffBookingHandler handler, CancellationToken cancellationToken)
    {
        return ApiResults.Ok(await handler.CancelAsync(id, user.ToActor(), cancellationToken), AgendaBookingDto.From);
    }

    /// <summary>Bloquea horas de un barbero.</summary>
    private static async Task<IResult> AddBlockAsync(AddBlockRequest request, ClaimsPrincipal user, BlockHandler handler, CancellationToken cancellationToken)
    {
        AddBlockCommand command = new(request.BarberId, request.Date, request.From, request.To, request.Reason);
        return ApiResults.Created(
            await handler.AddAsync(command, user.ToActor(), cancellationToken),
            AgendaBlockDto.From,
            block => $"/api/agenda/blocks/{block.Id}");
    }

    /// <summary>Quita un bloqueo.</summary>
    private static async Task<IResult> RemoveBlockAsync(Guid id, ClaimsPrincipal user, BlockHandler handler, CancellationToken cancellationToken)
    {
        Result result = await handler.RemoveAsync(id, user.ToActor(), cancellationToken);
        if (result.IsFailure)
        {
            return ApiResults.Problem(result.Error!);
        }

        return Results.NoContent();
    }
}
