using Barberia.Domain.Catalog;

namespace Barberia.Api.Contracts;

/// <summary>Servicio en la API, con el precio en centavos.</summary>
public sealed record ServiceResponse(Guid Id, string Name, int DurationMinutes, int PriceCents, bool IsActive)
{
    /// <summary>Convierte el servicio del dominio.</summary>
    public static ServiceResponse From(Service service)
    {
        return new ServiceResponse(service.Id, service.Name, service.DurationMinutes, service.Price.Cents, service.IsActive);
    }
}

/// <summary>Datos de un servicio que se crea o edita.</summary>
public sealed record SaveServiceRequest(string? Name, int DurationMinutes, int PriceCents, bool IsActive = true);
