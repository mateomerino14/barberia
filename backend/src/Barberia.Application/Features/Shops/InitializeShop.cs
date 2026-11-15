using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;

namespace Barberia.Application.Features.Shops;

/// <summary>Datos de arranque de una instalación nueva: nombre de la barbería y cuenta del dueño.</summary>
public sealed record InitializeShopCommand(string? ShopName, string? OwnerEmail, string? OwnerPassword);

/// <summary>Crea la barbería y la cuenta del dueño la primera vez que arranca la instalación.</summary>
public sealed class InitializeShopHandler(
    IShopRepository shops,
    IStaffAccountRepository accounts,
    IPasswordHasher hasher,
    IUnitOfWork unitOfWork)
{
    /// <summary>Crea lo que falte y no toca lo que ya existe.</summary>
    public async Task<Result> HandleAsync(InitializeShopCommand command, CancellationToken cancellationToken)
    {
        if (await shops.GetAsync(cancellationToken) is null)
        {
            Result<Shop> shop = Shop.Create(command.ShopName);
            if (shop.IsFailure)
            {
                return shop;
            }

            shops.Add(shop.Value);
        }

        if (!await accounts.AnyOwnerAsync(cancellationToken))
        {
            Result password = StaffAccount.ValidatePassword(command.OwnerPassword);
            if (password.IsFailure)
            {
                return password;
            }

            Result<StaffAccount> owner = StaffAccount.Create(command.OwnerEmail, hasher.Hash(command.OwnerPassword!), StaffRole.Owner, null);
            if (owner.IsFailure)
            {
                return owner;
            }

            accounts.Add(owner.Value);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
