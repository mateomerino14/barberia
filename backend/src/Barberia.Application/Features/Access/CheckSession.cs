using Barberia.Application.Ports;
using Barberia.Domain.Access;

namespace Barberia.Application.Features.Access;

/// <summary>Revisa en cada petición que el pase siga valiendo: la cuenta existe, no cambió su contraseña y no fue dada de baja.</summary>
public sealed class CheckSessionHandler(IStaffAccountRepository accounts, IBarberRepository barbers)
{
    /// <summary>Indica si la sesión de esa cuenta, con esa huella, sigue abierta.</summary>
    public async Task<bool> HandleAsync(Guid accountId, string? stamp, CancellationToken cancellationToken)
    {
        StaffAccount? account = await accounts.GetAsync(accountId, cancellationToken);
        if (account is null || !string.Equals(account.SessionStamp, stamp, StringComparison.Ordinal))
        {
            return false;
        }

        return await AccountAccess.IsAllowedAsync(account, barbers, cancellationToken);
    }
}
