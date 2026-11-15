namespace Barberia.Domain.Scheduling;

/// <summary>Estado de un turno.</summary>
public enum BookingStatus
{
    /// <summary>Reservado y por atender.</summary>
    Booked,

    /// <summary>El cliente vino y fue atendido.</summary>
    Attended,

    /// <summary>El cliente no vino.</summary>
    NoShow,

    /// <summary>Cancelado por el cliente o por la barbería.</summary>
    Cancelled,
}
