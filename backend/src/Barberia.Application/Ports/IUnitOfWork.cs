using Barberia.Domain.Common;

namespace Barberia.Application.Ports;

/// <summary>Puerto que confirma en una sola transacción los cambios de un caso de uso.</summary>
public interface IUnitOfWork
{
    /// <summary>Guarda todos los cambios pendientes.</summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>Guarda cambios de la agenda y devuelve un conflicto si otro turno tomó la misma hora al mismo tiempo.</summary>
    Task<Result> SaveScheduleChangesAsync(CancellationToken cancellationToken);
}
