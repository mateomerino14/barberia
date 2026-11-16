using Barberia.Api.Contracts;
using Barberia.Api.Http;
using Barberia.Application.Features.Bookings;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;

namespace Barberia.Api.Endpoints;

/// <summary>Rutas públicas para ver horas libres, reservar y manejar el turno desde su enlace.</summary>
public static class BookingEndpoints
{
    /// <summary>Registra las rutas de reservas.</summary>
    public static IEndpointRouteBuilder MapBookingEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/availability", GetAvailabilityAsync).RequireRateLimiting(RateLimits.PublicReads);
        routes.MapGet("/api/availability/days", GetDaysAsync).RequireRateLimiting(RateLimits.PublicReads);
        routes.MapPost("/api/bookings", CreateAsync).RequireRateLimiting(RateLimits.PublicWrites);
        routes.MapGet("/api/bookings/{token}", GetAsync).RequireRateLimiting(RateLimits.PublicReads);
        routes.MapPost("/api/bookings/{token}/cancel", CancelAsync).RequireRateLimiting(RateLimits.PublicWrites);
        routes.MapPut("/api/bookings/{token}", RescheduleAsync).RequireRateLimiting(RateLimits.PublicWrites);
        return routes;
    }

    /// <summary>Resume los días que se pueden reservar, con cuántas horas libres le quedan a cada uno.</summary>
    private static async Task<IResult> GetDaysAsync(
        Guid? serviceId,
        Guid? barberId,
        GetDaySummaryHandler handler,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<DaySummary>> days = await handler.HandleAsync(new GetDaySummaryQuery(serviceId, barberId), cancellationToken);
        return ApiResults.Ok(days, list => list.Select(day => new DaySummaryDto(day.Date, day.Open, day.FreeStarts)));
    }

    /// <summary>Devuelve las horas libres de un día para un servicio.</summary>
    private static async Task<IResult> GetAvailabilityAsync(
        DateOnly date,
        Guid serviceId,
        Guid? barberId,
        GetAvailabilityHandler handler,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyList<FreeSlot>> slots = await handler.HandleAsync(new GetAvailabilityQuery(date, serviceId, barberId), cancellationToken);
        return ApiResults.Ok(slots, list => list.Select(SlotDto.From));
    }

    /// <summary>Reserva un turno.</summary>
    private static async Task<IResult> CreateAsync(CreateBookingRequest request, CreateBookingHandler handler, CancellationToken cancellationToken)
    {
        CreateBookingCommand command = new(
            request.ServiceId,
            request.BarberId,
            request.Start,
            request.CustomerName,
            request.CustomerPhone,
            request.CustomerEmail);
        return ApiResults.Created(
            await handler.HandleAsync(command, cancellationToken),
            BookingConfirmation.From,
            booking => $"/api/bookings/{booking.ManageToken}");
    }

    /// <summary>Muestra un turno a partir de la clave de su enlace.</summary>
    private static async Task<IResult> GetAsync(string token, GetBookingHandler handler, CancellationToken cancellationToken)
    {
        return ApiResults.Ok(await handler.HandleAsync(token, cancellationToken), CustomerBookingResponse.From);
    }

    /// <summary>Cancela el turno, si todavía está dentro del plazo.</summary>
    private static async Task<IResult> CancelAsync(
        string token,
        CancelBookingHandler cancel,
        GetBookingHandler details,
        CancellationToken cancellationToken)
    {
        Result<Booking> result = await cancel.HandleAsync(token, cancellationToken);
        if (result.IsFailure)
        {
            return ApiResults.Problem(result.Error!);
        }

        return ApiResults.Ok(await details.HandleAsync(token, cancellationToken), CustomerBookingResponse.From);
    }

    /// <summary>Mueve el turno a otra hora libre, si todavía está dentro del plazo.</summary>
    private static async Task<IResult> RescheduleAsync(
        string token,
        RescheduleBookingRequest request,
        RescheduleBookingHandler reschedule,
        GetBookingHandler details,
        CancellationToken cancellationToken)
    {
        Result<Booking> result = await reschedule.HandleAsync(token, new RescheduleBookingCommand(request.Start, request.BarberId), cancellationToken);
        if (result.IsFailure)
        {
            return ApiResults.Problem(result.Error!);
        }

        return ApiResults.Ok(await details.HandleAsync(token, cancellationToken), CustomerBookingResponse.From);
    }
}
