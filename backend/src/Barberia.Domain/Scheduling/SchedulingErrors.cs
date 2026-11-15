using Barberia.Domain.Common;

namespace Barberia.Domain.Scheduling;

/// <summary>Errores de negocio de la agenda.</summary>
public static class SchedulingErrors
{
    /// <summary>El nombre del cliente está vacío o es muy largo.</summary>
    public static readonly Error InvalidCustomerName = Error.Validation("booking.customer_name", "Escribe tu nombre (hasta 80 letras).");

    /// <summary>El celular no tiene forma de número.</summary>
    public static readonly Error InvalidPhone = Error.Validation("booking.phone", "Escribe un celular válido, de 7 a 15 dígitos.");

    /// <summary>La hora elegida ya no está libre o no cumple las reglas.</summary>
    public static readonly Error SlotUnavailable = Error.Conflict("booking.slot_unavailable", "Esa hora ya no está libre. Elige otra.");

    /// <summary>El barbero no hace ese servicio o no recibe turnos.</summary>
    public static readonly Error BarberUnavailable = Error.Validation("booking.barber_unavailable", "Ese barbero no hace ese servicio.");

    /// <summary>El servicio no existe o está oculto.</summary>
    public static readonly Error ServiceUnavailable = Error.Validation("booking.service_unavailable", "Ese servicio no se puede reservar.");

    /// <summary>No se encontró el turno.</summary>
    public static readonly Error NotFound = Error.NotFound("booking.not_found", "No encontramos ese turno.");

    /// <summary>Ya pasó el plazo para que el cliente cancele o cambie.</summary>
    public static readonly Error TooLateToChange = Error.Conflict("booking.too_late", "Ya no se puede cambiar ni cancelar este turno por aquí. Llama a la barbería.");

    /// <summary>El turno ya no está reservado: fue atendido, faltó o se canceló.</summary>
    public static readonly Error NotActive = Error.Conflict("booking.not_active", "Este turno ya no está activo.");

    /// <summary>El motivo del bloqueo es muy largo.</summary>
    public static readonly Error InvalidReason = Error.Validation("block.reason", "El motivo tiene hasta 120 letras.");

    /// <summary>No se encontró el bloqueo.</summary>
    public static readonly Error BlockNotFound = Error.NotFound("block.not_found", "Ese bloqueo no existe.");
}
