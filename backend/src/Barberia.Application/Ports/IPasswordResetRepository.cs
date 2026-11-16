using Barberia.Domain.Access;

namespace Barberia.Application.Ports;

/// <summary>Puerto de persistencia de los enlaces para cambiar la contraseña.</summary>
public interface IPasswordResetRepository
{
    /// <summary>Busca un enlace por la huella de su clave.</summary>
    Task<PasswordReset?> FindByFingerprintAsync(string fingerprint, CancellationToken cancellationToken);

    /// <summary>Lista los enlaces de una cuenta que todavía no se usaron.</summary>
    Task<IReadOnlyList<PasswordReset>> ListUnusedAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>Agrega un enlace.</summary>
    void Add(PasswordReset reset);
}
