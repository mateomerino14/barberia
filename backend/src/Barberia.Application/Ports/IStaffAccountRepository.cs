using Barberia.Domain.Access;

namespace Barberia.Application.Ports;

/// <summary>Puerto de persistencia de las cuentas del panel.</summary>
public interface IStaffAccountRepository
{
    /// <summary>Busca una cuenta por su correo ya normalizado.</summary>
    Task<StaffAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>Busca una cuenta por su id.</summary>
    Task<StaffAccount?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Busca la cuenta vinculada a un barbero.</summary>
    Task<StaffAccount?> FindByBarberAsync(Guid barberId, CancellationToken cancellationToken);

    /// <summary>Indica si ya hay alguna cuenta de dueño.</summary>
    Task<bool> AnyOwnerAsync(CancellationToken cancellationToken);

    /// <summary>Agrega una cuenta.</summary>
    void Add(StaffAccount account);
}
