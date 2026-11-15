namespace Barberia.Application.Ports;

/// <summary>Puerto que confirma en una sola transacción los cambios de un caso de uso.</summary>
public interface IUnitOfWork
{
    /// <summary>Guarda todos los cambios pendientes.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
