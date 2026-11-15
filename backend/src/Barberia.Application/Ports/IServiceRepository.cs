using Barberia.Domain.Catalog;

namespace Barberia.Application.Ports;

/// <summary>Puerto de persistencia del catálogo de servicios.</summary>
public interface IServiceRepository
{
    /// <summary>Busca un servicio por su Id.</summary>
    Task<Service?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Lista los servicios ordenados por nombre, solo los activos si se pide.</summary>
    Task<IReadOnlyList<Service>> ListAsync(bool onlyActive, CancellationToken cancellationToken);

    /// <summary>Indica si existen todos los servicios de la lista.</summary>
    Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>Agrega un servicio.</summary>
    void Add(Service service);
}
