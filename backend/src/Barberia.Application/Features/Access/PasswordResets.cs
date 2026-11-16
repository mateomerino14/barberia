using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Common;
using Barberia.Domain.Shops;

namespace Barberia.Application.Features.Access;

/// <summary>Cambio de contraseña con la clave del enlace que llegó por correo.</summary>
public sealed record ResetPasswordCommand(string? Token, string? NewPassword);

/// <summary>Dice si se puede recuperar la contraseña por correo en esta instalación.</summary>
public sealed class PasswordResetStatusHandler(IAccessNotifier notifier)
{
    /// <summary>Devuelve si hay correos configurados.</summary>
    public bool Handle()
    {
        return notifier.IsAvailable;
    }
}

/// <summary>Manda el enlace para crear una contraseña nueva, sin revelar si el correo tiene cuenta.</summary>
public sealed class RequestPasswordResetHandler(
    IStaffAccountRepository accounts,
    IBarberRepository barbers,
    IShopRepository shops,
    IPasswordResetRepository resets,
    IAccessNotifier notifier,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    /// <summary>Si el correo es de una cuenta con acceso, crea el enlace y lo manda; si no, no hace nada.</summary>
    public async Task HandleAsync(string? email, CancellationToken cancellationToken)
    {
        Result<string> normalized = EmailAddress.Normalize(email);
        if (!notifier.IsAvailable || normalized.IsFailure)
        {
            return;
        }

        StaffAccount? account = await accounts.FindByEmailAsync(normalized.Value, cancellationToken);
        if (account is null || !await AccountAccess.IsAllowedAsync(account, barbers, cancellationToken))
        {
            return;
        }

        PasswordResetLink link = PasswordReset.Issue(account.Id, clock.Now);
        resets.Add(link.Reset);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        Shop? shop = await shops.GetAsync(cancellationToken);
        PasswordResetNotice notice = new(account.Email, shop?.Name ?? string.Empty, link.Token, link.Reset.ExpiresAt);
        await notifier.SendPasswordResetAsync(notice, cancellationToken);
    }
}

/// <summary>Pone la contraseña nueva con un enlace válido, lo anula junto con los demás de la cuenta y cierra las sesiones abiertas.</summary>
public sealed class ResetPasswordHandler(
    IPasswordResetRepository resets,
    IStaffAccountRepository accounts,
    IPasswordHasher hasher,
    IClock clock,
    IUnitOfWork unitOfWork)
{
    /// <summary>Cambia la contraseña o explica por qué no se pudo.</summary>
    public async Task<Result> HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
        {
            return AccessErrors.ResetLinkInvalid;
        }

        PasswordReset? reset = await resets.FindByFingerprintAsync(PasswordReset.Fingerprint(command.Token), cancellationToken);
        if (reset is null || !reset.IsUsable(clock.Now))
        {
            return AccessErrors.ResetLinkInvalid;
        }

        Result valid = StaffAccount.ValidatePassword(command.NewPassword);
        if (valid.IsFailure)
        {
            return valid;
        }

        StaffAccount? account = await accounts.GetAsync(reset.AccountId, cancellationToken);
        if (account is null)
        {
            return AccessErrors.ResetLinkInvalid;
        }

        account.ChangePassword(hasher.Hash(command.NewPassword!));
        foreach (PasswordReset pending in await resets.ListUnusedAsync(account.Id, cancellationToken))
        {
            pending.MarkUsed(clock.Now);
        }

        reset.MarkUsed(clock.Now);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
