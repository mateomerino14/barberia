using Barberia.Application.Features.Agenda;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;

namespace Barberia.Api.Contracts;

/// <summary>Barbero en la agenda, con su horario del día en hora de La Paz.</summary>
public sealed record AgendaBarberDto(Guid Id, string Name, DateTimeOffset? Opens, DateTimeOffset? Closes);

/// <summary>Turno en la agenda del panel.</summary>
public sealed record AgendaBookingDto(
    Guid Id,
    int Number,
    Guid BarberId,
    Guid ServiceId,
    string ServiceName,
    int PriceCents,
    DateTimeOffset Start,
    DateTimeOffset End,
    string Status,
    string CustomerName,
    string CustomerPhone,
    bool IsWalkIn)
{
    /// <summary>Convierte el turno del dominio.</summary>
    public static AgendaBookingDto From(Booking booking)
    {
        return new AgendaBookingDto(
            booking.Id,
            booking.Number,
            booking.BarberId,
            booking.ServiceId,
            booking.ServiceName,
            booking.PriceCents,
            BusinessTime.ToLocal(booking.Range.Start),
            BusinessTime.ToLocal(booking.Range.End),
            booking.Status.ToString(),
            booking.Customer.Name,
            booking.Customer.Phone,
            booking.IsWalkIn);
    }
}

/// <summary>Bloqueo en la agenda del panel.</summary>
public sealed record AgendaBlockDto(Guid Id, Guid BarberId, DateTimeOffset Start, DateTimeOffset End, string Reason)
{
    /// <summary>Convierte el bloqueo del dominio.</summary>
    public static AgendaBlockDto From(Block block)
    {
        return new AgendaBlockDto(block.Id, block.BarberId, BusinessTime.ToLocal(block.Range.Start), BusinessTime.ToLocal(block.Range.End), block.Reason);
    }
}

/// <summary>Agenda de un día.</summary>
public sealed record AgendaResponse(
    DateOnly Date,
    IReadOnlyList<AgendaBarberDto> Barbers,
    IReadOnlyList<AgendaBookingDto> Bookings,
    IReadOnlyList<AgendaBlockDto> Blocks)
{
    /// <summary>Convierte la agenda del caso de uso.</summary>
    public static AgendaResponse From(AgendaDay day)
    {
        return new AgendaResponse(
            day.Date,
            [.. day.Columns.Select(column => new AgendaBarberDto(
                column.Barber.Id,
                column.Barber.Name,
                LocalOrNull(column.Workday?.Start),
                LocalOrNull(column.Workday?.End)))],
            [.. day.Bookings.Select(AgendaBookingDto.From)],
            [.. day.Blocks.Select(AgendaBlockDto.From)]);
    }

    /// <summary>Pasa un instante a la hora de La Paz, o deja null.</summary>
    private static DateTimeOffset? LocalOrNull(DateTimeOffset? instant)
    {
        if (instant is null)
        {
            return null;
        }

        return BusinessTime.ToLocal(instant.Value);
    }
}

/// <summary>Turno que anota la barbería.</summary>
public sealed record CreateStaffBookingRequest(
    Guid ServiceId,
    Guid BarberId,
    DateTimeOffset Start,
    string? CustomerName,
    string? CustomerPhone,
    bool IsWalkIn);

/// <summary>Estado nuevo de un turno.</summary>
public sealed record SetBookingStatusRequest(BookingStatus Status);

/// <summary>Bloqueo nuevo.</summary>
public sealed record AddBlockRequest(Guid BarberId, DateOnly Date, TimeOnly From, TimeOnly To, string? Reason);
