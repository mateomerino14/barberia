using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;

namespace Barberia.Application.Features.Shops;

/// <summary>Fecha que se marca como cerrada y su motivo.</summary>
public sealed record AddClosedDayCommand(DateOnly Date, string? Reason);

/// <summary>Marca una fecha como cerrada.</summary>
public sealed class AddClosedDayHandler(IShopRepository shops, IUnitOfWork unitOfWork)
{
    /// <summary>Agrega el día cerrado si no estaba.</summary>
    public async Task<Result<Shop>> HandleAsync(AddClosedDayCommand command, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        Result result = shop.AddClosedDay(command.Date, command.Reason);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return shop;
    }
}

/// <summary>Quita una fecha cerrada.</summary>
public sealed class RemoveClosedDayHandler(IShopRepository shops, IUnitOfWork unitOfWork)
{
    /// <summary>Quita el día cerrado si existía.</summary>
    public async Task<Result<Shop>> HandleAsync(DateOnly date, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        Result result = shop.RemoveClosedDay(date);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return shop;
    }
}
