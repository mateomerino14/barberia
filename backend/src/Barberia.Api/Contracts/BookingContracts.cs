using Barberia.Application.Features.Bookings;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;

namespace Barberia.Api.Contracts;

/// <summary>Hora libre con los barberos que pueden atender, en hora de La Paz.</summary>
public sealed record SlotDto(DateTimeOffset Start, IReadOnlyList<Guid> BarberIds)
{
    /// <summary>Convierte la hora libre del caso de uso.</summary>
    public static SlotDto From(FreeSlot slot)
    {
        return new SlotDto(BusinessTime.ToLocal(slot.Start), slot.BarberIds);
    }
}

/// <summary>Reserva nueva desde la página pública.</summary>
public sealed record CreateBookingRequest(
    Guid ServiceId,
    Guid? BarberId,
    DateTimeOffset Start,
    string? CustomerName,
    string? CustomerPhone,
    string? CustomerEmail);

/// <summary>Turno recién reservado, con la clave para volver a verlo.</summary>
public sealed record BookingConfirmation(
    int Number,
    string Token,
    Guid BarberId,
    string ServiceName,
    int PriceCents,
    DateTimeOffset Start,
    DateTimeOffset End)
{
    /// <summary>Convierte el turno del dominio.</summary>
    public static BookingConfirmation From(Booking booking)
    {
        return new BookingConfirmation(
            booking.Number,
            booking.ManageToken,
            booking.BarberId,
            booking.ServiceName,
            booking.PriceCents,
            BusinessTime.ToLocal(booking.Range.Start),
            BusinessTime.ToLocal(booking.Range.End));
    }
}

/// <summary>Turno tal como lo ve el cliente en su enlace.</summary>
public sealed record CustomerBookingResponse(
    int Number,
    string Status,
    Guid ServiceId,
    string ServiceName,
    int PriceCents,
    Guid BarberId,
    string BarberName,
    string CustomerName,
    DateTimeOffset Start,
    DateTimeOffset End,
    DateTimeOffset ChangeUntil)
{
    /// <summary>Convierte el turno con sus datos de contexto.</summary>
    public static CustomerBookingResponse From(BookingDetails details)
    {
        Booking booking = details.Booking;
        return new CustomerBookingResponse(
            booking.Number,
            booking.Status.ToString(),
            booking.ServiceId,
            booking.ServiceName,
            booking.PriceCents,
            booking.BarberId,
            details.BarberName,
            booking.Customer.Name,
            BusinessTime.ToLocal(booking.Range.Start),
            BusinessTime.ToLocal(booking.Range.End),
            BusinessTime.ToLocal(booking.Range.Start.AddMinutes(-details.CancelLimitMinutes)));
    }
}

/// <summary>Nueva hora para un turno, y opcionalmente otro barbero.</summary>
public sealed record RescheduleBookingRequest(DateTimeOffset Start, Guid? BarberId);
