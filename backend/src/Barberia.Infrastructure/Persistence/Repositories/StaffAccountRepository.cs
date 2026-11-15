using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Infrastructure.Persistence.Repositories;

/// <summary>Adaptador de PostgreSQL para las cuentas del panel.</summary>
public sealed class StaffAccountRepository(BarberiaDbContext context) : IStaffAccountRepository
{
    /// <summary>Busca una cuenta por su correo.</summary>
    public Task<StaffAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return context.StaffAccounts.FirstOrDefaultAsync(account => account.Email == email, cancellationToken);
    }

    /// <summary>Busca una cuenta por su id.</summary>
    public Task<StaffAccount?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.StaffAccounts.FirstOrDefaultAsync(account => account.Id == id, cancellationToken);
    }

    /// <summary>Busca la cuenta vinculada a un barbero.</summary>
    public Task<StaffAccount?> FindByBarberAsync(Guid barberId, CancellationToken cancellationToken)
    {
        return context.StaffAccounts.FirstOrDefaultAsync(account => account.BarberId == barberId, cancellationToken);
    }

    /// <summary>Indica si ya hay alguna cuenta de dueño.</summary>
    public Task<bool> AnyOwnerAsync(CancellationToken cancellationToken)
    {
        return context.StaffAccounts.AnyAsync(account => account.Role == StaffRole.Owner, cancellationToken);
    }

    /// <summary>Agrega una cuenta.</summary>
    public void Add(StaffAccount account)
    {
        context.StaffAccounts.Add(account);
    }
}
