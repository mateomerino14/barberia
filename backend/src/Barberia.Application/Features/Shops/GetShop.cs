using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;

namespace Barberia.Application.Features.Shops;

/// <summary>Devuelve la barbería con su horario y sus reglas.</summary>
public sealed class GetShopHandler(IShopRepository shops)
{
    /// <summary>Busca la barbería configurada.</summary>
    public async Task<Result<Shop>> HandleAsync(CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        return shop;
    }
}
