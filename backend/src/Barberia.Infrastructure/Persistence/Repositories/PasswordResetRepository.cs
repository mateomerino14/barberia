using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Infrastructure.Persistence.Repositories;

/// <summary>Adaptador de PostgreSQL para los enlaces de cambio de contraseña.</summary>
public sealed class PasswordResetRepository(BarberiaDbContext context) : IPasswordResetRepository
{
    /// <summary>Busca un enlace por la huella de su clave.</summary>
    public Task<PasswordReset?> FindByFingerprintAsync(string fingerprint, CancellationToken cancellationToken)
    {
        return context.PasswordResets.FirstOrDefaultAsync(reset => reset.TokenHash == fingerprint, cancellationToken);
    }

    /// <summary>Lista los enlaces de una cuenta que todavía no se usaron.</summary>
    public async Task<IReadOnlyList<PasswordReset>> ListUnusedAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await context.PasswordResets.Where(reset => reset.AccountId == accountId && reset.UsedAt == null).ToListAsync(cancellationToken);
    }

    /// <summary>Agrega un enlace.</summary>
    public void Add(PasswordReset reset)
    {
        context.PasswordResets.Add(reset);
    }
}
