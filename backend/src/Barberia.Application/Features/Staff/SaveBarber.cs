using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Common;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Staff;

/// <summary>Datos de un barbero: nombre, si está activo y qué servicios hace.</summary>
public sealed record SaveBarberCommand(string? Name, bool IsActive, IReadOnlyList<Guid> ServiceIds);

/// <summary>Cuenta opcional para que el barbero entre al panel.</summary>
public sealed record BarberAccountInput(string? Email, string? Password);

/// <summary>Crea un barbero y, si se pide, su cuenta del panel.</summary>
public sealed class CreateBarberHandler(
    IBarberRepository barbers,
    IServiceRepository services,
    IStaffAccountRepository accounts,
    IPasswordHasher hasher,
    IUnitOfWork unitOfWork)
{
    /// <summary>Guarda el barbero y su cuenta en una sola transacción.</summary>
    public async Task<Result<Barber>> HandleAsync(
        SaveBarberCommand command,
        BarberAccountInput? account,
        CancellationToken cancellationToken)
    {
        if (!await services.AllExistAsync(command.ServiceIds, cancellationToken))
        {
            return StaffErrors.UnknownService;
        }

        Result<Barber> barber = Barber.Create(command.Name);
        if (barber.IsFailure)
        {
            return barber;
        }

        barber.Value.AssignServices(command.ServiceIds);
        barber.Value.SetActive(command.IsActive);
        if (account is not null)
        {
            Result accountResult = await CreateAccountAsync(barber.Value, account, cancellationToken);
            if (accountResult.IsFailure)
            {
                return accountResult.Error!;
            }
        }

        barbers.Add(barber.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return barber;
    }

    /// <summary>Crea la cuenta del barbero si el correo está libre y la contraseña es válida.</summary>
    private async Task<Result> CreateAccountAsync(Barber barber, BarberAccountInput input, CancellationToken cancellationToken)
    {
        Result password = StaffAccount.ValidatePassword(input.Password);
        if (password.IsFailure)
        {
            return password;
        }

        Result<string> email = EmailAddress.Normalize(input.Email);
        if (email.IsFailure)
        {
            return email;
        }

        if (await accounts.FindByEmailAsync(email.Value, cancellationToken) is not null)
        {
            return AccessErrors.EmailTaken;
        }

        Result<StaffAccount> account = StaffAccount.Create(email.Value, hasher.Hash(input.Password!), StaffRole.Barber, barber.Id);
        if (account.IsFailure)
        {
            return account;
        }

        accounts.Add(account.Value);
        return Result.Success();
    }
}

/// <summary>Edita el nombre, el estado y los servicios de un barbero.</summary>
public sealed class UpdateBarberHandler(IBarberRepository barbers, IServiceRepository services, IUnitOfWork unitOfWork)
{
    /// <summary>Guarda los cambios si el barbero y los servicios existen.</summary>
    public async Task<Result<Barber>> HandleAsync(Guid id, SaveBarberCommand command, CancellationToken cancellationToken)
    {
        Barber? barber = await barbers.GetAsync(id, cancellationToken);
        if (barber is null)
        {
            return StaffErrors.NotFound;
        }

        if (!await services.AllExistAsync(command.ServiceIds, cancellationToken))
        {
            return StaffErrors.UnknownService;
        }

        Result rename = barber.Rename(command.Name);
        if (rename.IsFailure)
        {
            return rename.Error!;
        }

        barber.AssignServices(command.ServiceIds);
        barber.SetActive(command.IsActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return barber;
    }
}
