using Barberia.Domain.Common;

namespace Barberia.Domain.Shops;

/// <summary>Errores de negocio del módulo de la barbería.</summary>
public static class ShopErrors
{
    /// <summary>El nombre está vacío o es muy largo.</summary>
    public static readonly Error InvalidName = Error.Validation("shop.name", "El nombre de la barbería es obligatorio y tiene hasta 80 letras.");

    /// <summary>La dirección es muy larga.</summary>
    public static readonly Error InvalidAddress = Error.Validation("shop.address", "La dirección tiene hasta 160 letras.");

    /// <summary>El teléfono es muy largo.</summary>
    public static readonly Error InvalidPhone = Error.Validation("shop.phone", "El teléfono tiene hasta 20 caracteres.");

    /// <summary>El mensaje de bienvenida es muy largo.</summary>
    public static readonly Error InvalidWelcome = Error.Validation("shop.welcome", "El mensaje de bienvenida tiene hasta 280 letras.");

    /// <summary>El motivo del día cerrado es muy largo.</summary>
    public static readonly Error InvalidReason = Error.Validation("shop.reason", "El motivo tiene hasta 120 letras.");

    /// <summary>Ese día ya estaba marcado como cerrado.</summary>
    public static readonly Error ClosedDayExists = Error.Conflict("shop.closed_day_exists", "Ese día ya está marcado como cerrado.");

    /// <summary>Ese día no estaba marcado como cerrado.</summary>
    public static readonly Error ClosedDayNotFound = Error.NotFound("shop.closed_day_not_found", "Ese día no está marcado como cerrado.");

    /// <summary>La barbería todavía no está configurada.</summary>
    public static readonly Error NotConfigured = Error.NotFound("shop.not_configured", "La barbería todavía no está configurada.");
}
