using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Staff;

/// <summary>Horario semanal nuevo de un barbero.</summary>
public sealed record SetBarberScheduleCommand(IReadOnlyList<HoursInput> Hours);

/// <summary>Reemplaza el horario de un barbero, que debe caber en el de la barbería.</summary>
public sealed class SetBarberScheduleHandler(IBarberRepository barbers, IShopRepository shops, IUnitOfWork unitOfWork)
{
    /// <summary>Guarda el horario si cada día es válido y cabe en el horario de atención.</summary>
    public async Task<Result<Barber>> HandleAsync(Guid id, SetBarberScheduleCommand command, CancellationToken cancellationToken)
    {
        Barber? barber = await barbers.GetAsync(id, cancellationToken);
        if (barber is null)
        {
            return StaffErrors.NotFound;
        }

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

        Result result = barber.SetSchedule([.. hours.Value], [.. shop.OpeningHours]);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return barber;
    }
}
