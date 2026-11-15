using Barberia.Application.Ports;
using Barberia.Domain.Catalog;

namespace Barberia.Application.Features.Catalog;

/// <summary>Lista los servicios: todos para el panel, solo los activos para el cliente.</summary>
public sealed class ListServicesHandler(IServiceRepository services)
{
    /// <summary>Devuelve los servicios ordenados por nombre.</summary>
    public Task<IReadOnlyList<Service>> HandleAsync(bool onlyActive, CancellationToken cancellationToken)
    {
        return services.ListAsync(onlyActive, cancellationToken);
    }
}
