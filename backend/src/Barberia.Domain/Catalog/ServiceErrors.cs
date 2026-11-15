using Barberia.Domain.Common;

namespace Barberia.Domain.Catalog;

/// <summary>Errores de negocio del catálogo de servicios.</summary>
public static class ServiceErrors
{
    /// <summary>El nombre está vacío o es muy largo.</summary>
    public static readonly Error InvalidName = Error.Validation("service.name", "El nombre del servicio es obligatorio y tiene hasta 80 letras.");

    /// <summary>La duración no está en el rango permitido.</summary>
    public static readonly Error InvalidDuration = Error.Validation("service.duration", "La duración va de 5 minutos a 8 horas, en pasos de 5 minutos.");

    /// <summary>El servicio buscado no existe.</summary>
    public static readonly Error NotFound = Error.NotFound("service.not_found", "Ese servicio no existe.");
}
