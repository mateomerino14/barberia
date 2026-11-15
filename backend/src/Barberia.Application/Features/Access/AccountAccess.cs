using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Access;

/// <summary>Regla común para entrar al panel: un barbero dado de baja ya no tiene acceso.</summary>
internal static class AccountAccess
{
    /// <summary>Indica si la cuenta puede usar el panel ahora.</summary>
    public static async Task<bool> IsAllowedAsync(StaffAccount account, IBarberRepository barbers, CancellationToken cancellationToken)
    {
        if (account.BarberId is null)
        {
            return account.Role == StaffRole.Owner;
        }

        Barber? barber = await barbers.GetAsync(account.BarberId.Value, cancellationToken);
        return barber is { IsActive: true };
    }
}
