using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;

namespace Barberia.Application.Features.Shops;

/// <summary>Datos de la barbería y reglas de reserva que edita el dueño.</summary>
public sealed record UpdateShopCommand(
    string? Name,
    string? Address,
    string? Phone,
    string? WelcomeMessage,
    int MinLeadMinutes,
    int MaxDaysAhead,
    int CancelLimitMinutes,
    ShopPalette Palette);

/// <summary>Actualiza los datos de la barbería y sus reglas de reserva.</summary>
public sealed class UpdateShopHandler(IShopRepository shops, IUnitOfWork unitOfWork)
{
    /// <summary>Guarda los cambios si todos los datos son válidos.</summary>
    public async Task<Result<Shop>> HandleAsync(UpdateShopCommand command, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        Result<BookingRules> rules = BookingRules.Create(command.MinLeadMinutes, command.MaxDaysAhead, command.CancelLimitMinutes);
        if (rules.IsFailure)
        {
            return rules.Error!;
        }

        Result profile = shop.UpdateProfile(command.Name, command.Address, command.Phone, command.WelcomeMessage);
        if (profile.IsFailure)
        {
            return profile.Error!;
        }

        shop.UpdateRules(rules.Value);
        shop.ChangePalette(command.Palette);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return shop;
    }
}
