using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Common;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Access;

/// <summary>Cambio de la propia contraseña: hace falta la actual.</summary>
public sealed record ChangePasswordCommand(Guid AccountId, string? CurrentPassword, string? NewPassword);

/// <summary>Acceso de un barbero: correo para crear la cuenta si no tiene, y la contraseña nueva.</summary>
public sealed record BarberAccessCommand(Guid BarberId, string? Email, string? Password);

/// <summary>Cambia la contraseña de quien está en el panel y le entrega un pase nuevo; las demás sesiones se cierran.</summary>
public sealed class ChangePasswordHandler(IStaffAccountRepository accounts, IPasswordHasher hasher, ITokenIssuer issuer, IUnitOfWork unitOfWork)
{
    /// <summary>Verifica la contraseña actual, guarda la nueva y abre una sesión nueva.</summary>
    public async Task<Result<LoginResult>> HandleAsync(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        StaffAccount? account = await accounts.GetAsync(command.AccountId, cancellationToken);
        if (account is null)
        {
            return AccessErrors.AccountNotFound;
        }

        if (string.IsNullOrEmpty(command.CurrentPassword) || !hasher.Verify(account.PasswordHash, command.CurrentPassword))
        {
            return AccessErrors.WrongCurrentPassword;
        }

        Result valid = StaffAccount.ValidatePassword(command.NewPassword);
        if (valid.IsFailure)
        {
            return valid.Error!;
        }

        account.ChangePassword(hasher.Hash(command.NewPassword!));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new LoginResult(issuer.Issue(account), account);
    }
}

/// <summary>El dueño le da acceso al panel a un barbero o le pone una contraseña nueva si la olvidó.</summary>
public sealed class BarberAccessHandler(IBarberRepository barbers, IStaffAccountRepository accounts, IPasswordHasher hasher, IUnitOfWork unitOfWork)
{
    /// <summary>Crea la cuenta del barbero o reemplaza su contraseña; en ambos casos devuelve la cuenta.</summary>
    public async Task<Result<StaffAccount>> HandleAsync(BarberAccessCommand command, CancellationToken cancellationToken)
    {
        Barber? barber = await barbers.GetAsync(command.BarberId, cancellationToken);
        if (barber is null)
        {
            return StaffErrors.NotFound;
        }

        Result valid = StaffAccount.ValidatePassword(command.Password);
        if (valid.IsFailure)
        {
            return valid.Error!;
        }

        string hash = hasher.Hash(command.Password!);
        StaffAccount? existing = await accounts.FindByBarberAsync(barber.Id, cancellationToken);
        if (existing is not null)
        {
            existing.ChangePassword(hash);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return existing;
        }

        Result<StaffAccount> created = await CreateAsync(barber, command.Email, hash, cancellationToken);
        if (created.IsFailure)
        {
            return created;
        }

        accounts.Add(created.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return created;
    }

    /// <summary>Arma la cuenta nueva si el correo es válido y está libre.</summary>
    private async Task<Result<StaffAccount>> CreateAsync(Barber barber, string? email, string hash, CancellationToken cancellationToken)
    {
        Result<string> normalized = EmailAddress.Normalize(email);
        if (normalized.IsFailure)
        {
            return normalized.Error!;
        }

        if (await accounts.FindByEmailAsync(normalized.Value, cancellationToken) is not null)
        {
            return AccessErrors.EmailTaken;
        }

        return StaffAccount.Create(normalized.Value, hash, StaffRole.Barber, barber.Id);
    }
}

/// <summary>Lista qué barberos tienen acceso al panel y con qué correo.</summary>
public sealed class ListBarberAccessHandler(IStaffAccountRepository accounts)
{
    /// <summary>Devuelve las cuentas de los barberos.</summary>
    public Task<IReadOnlyList<StaffAccount>> HandleAsync(CancellationToken cancellationToken)
    {
        return accounts.ListBarberAccountsAsync(cancellationToken);
    }
}
