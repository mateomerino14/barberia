using Barberia.Domain.Staff;

namespace Barberia.Api.Contracts;

/// <summary>Ausencia en la API.</summary>
public sealed record TimeOffDto(Guid Id, DateOnly From, DateOnly To, string Reason);

/// <summary>Barbero tal como lo ve el cliente: nombre, servicios y horario.</summary>
public sealed record PublicBarberResponse(Guid Id, string Name, IReadOnlyList<Guid> ServiceIds, Guid? PhotoImageId)
{
    /// <summary>Convierte el barbero del dominio sin datos internos.</summary>
    public static PublicBarberResponse From(Barber barber)
    {
        return new PublicBarberResponse(barber.Id, barber.Name, barber.ServiceIds, barber.PhotoImageId);
    }
}

/// <summary>Foto nueva del barbero, o null para quitarla.</summary>
public sealed record SetPhotoRequest(Guid? ImageId);

/// <summary>Barbero tal como lo ve el panel, con horario y ausencias.</summary>
public sealed record BarberResponse(
    Guid Id,
    string Name,
    bool IsActive,
    IReadOnlyList<Guid> ServiceIds,
    IReadOnlyList<HoursDto> Schedule,
    IReadOnlyList<TimeOffDto> TimeOff,
    Guid? PhotoImageId)
{
    /// <summary>Convierte el barbero del dominio.</summary>
    public static BarberResponse From(Barber barber)
    {
        return new BarberResponse(
            barber.Id,
            barber.Name,
            barber.IsActive,
            barber.ServiceIds,
            [.. barber.Schedule.Select(HoursDto.From)],
            [.. barber.TimeOff.OrderBy(item => item.From).Select(item => new TimeOffDto(item.Id, item.From, item.To, item.Reason))],
            barber.PhotoImageId);
    }
}

/// <summary>Cuenta opcional para el barbero nuevo.</summary>
public sealed record BarberAccountRequest(string? Email, string? Password);

/// <summary>Datos de un barbero que se crea o edita.</summary>
public sealed record SaveBarberRequest(
    string? Name,
    IReadOnlyList<Guid>? ServiceIds,
    bool IsActive = true,
    BarberAccountRequest? Account = null);

/// <summary>Ausencia nueva.</summary>
public sealed record AddTimeOffRequest(DateOnly From, DateOnly To, string? Reason);
