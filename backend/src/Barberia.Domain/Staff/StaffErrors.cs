using Barberia.Domain.Common;

namespace Barberia.Domain.Staff;

/// <summary>Errores de negocio del equipo de barberos.</summary>
public static class StaffErrors
{
    /// <summary>El nombre está vacío o es muy largo.</summary>
    public static readonly Error InvalidName = Error.Validation("barber.name", "El nombre del barbero es obligatorio y tiene hasta 80 letras.");

    /// <summary>El horario del barbero se sale del horario de la barbería.</summary>
    public static readonly Error OutsideShopHours = Error.Validation("barber.outside_shop_hours", "El horario del barbero tiene que estar dentro del horario de la barbería.");

    /// <summary>La ausencia termina antes de empezar.</summary>
    public static readonly Error InvalidTimeOff = Error.Validation("barber.time_off", "La ausencia tiene que terminar el mismo día o después de empezar.");

    /// <summary>El motivo es muy largo.</summary>
    public static readonly Error InvalidReason = Error.Validation("barber.reason", "El motivo tiene hasta 120 letras.");

    /// <summary>La ausencia buscada no existe.</summary>
    public static readonly Error TimeOffNotFound = Error.NotFound("barber.time_off_not_found", "Esa ausencia no existe.");

    /// <summary>El barbero buscado no existe.</summary>
    public static readonly Error NotFound = Error.NotFound("barber.not_found", "Ese barbero no existe.");

    /// <summary>Uno de los servicios asignados no existe.</summary>
    public static readonly Error UnknownService = Error.Validation("barber.unknown_service", "Uno de los servicios elegidos no existe.");
}
