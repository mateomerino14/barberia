using Barberia.Domain.Media;

namespace Barberia.Application.Ports;

/// <summary>Puerto de persistencia de las imágenes.</summary>
public interface IImageRepository
{
    /// <summary>Busca una imagen con su contenido.</summary>
    Task<StoredImage?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Indica si existen todas las imágenes indicadas.</summary>
    Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>Agrega una imagen.</summary>
    void Add(StoredImage image);

    /// <summary>Borra las imágenes que ya no se usan.</summary>
    Task RemoveAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
}
