using Barberia.Domain.Shops;

namespace Barberia.Application.Ports;

/// <summary>Puerto de persistencia de la barbería; cada instalación tiene una sola.</summary>
public interface IShopRepository
{
    /// <summary>Devuelve la barbería, o nulo si todavía no se configuró.</summary>
    Task<Shop?> GetAsync(CancellationToken cancellationToken);

    /// <summary>Agrega la barbería.</summary>
    void Add(Shop shop);
}
