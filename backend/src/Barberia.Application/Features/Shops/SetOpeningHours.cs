using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;

namespace Barberia.Application.Features.Shops;

/// <summary>Horario semanal nuevo de la barbería.</summary>
public sealed record SetOpeningHoursCommand(IReadOnlyList<HoursInput> Hours);

/// <summary>Reemplaza el horario semanal de atención.</summary>
public sealed class SetOpeningHoursHandler(IShopRepository shops, IUnitOfWork unitOfWork)
{
    /// <summary>Guarda el horario si cada día es válido y no se repite.</summary>
    public async Task<Result<Shop>> HandleAsync(SetOpeningHoursCommand command, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        Result<IReadOnlyList<DailyHours>> hours = command.Hours.ToDailyHours();
        if (hours.IsFailure)
        {
            return hours.Error!;
        }

        Result result = shop.SetOpeningHours([.. hours.Value]);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return shop;
    }
}
