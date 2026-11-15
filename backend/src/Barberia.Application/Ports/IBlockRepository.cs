using Barberia.Domain.Scheduling;

namespace Barberia.Application.Ports;

/// <summary>Puerto de persistencia de los bloqueos de agenda.</summary>
public interface IBlockRepository
{
    /// <summary>Busca un bloqueo por su Id.</summary>
    Task<Block?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Lista los bloqueos que tocan el intervalo, de un barbero o de todos.</summary>
    Task<IReadOnlyList<Block>> ListAsync(TimeRange range, Guid? barberId, CancellationToken cancellationToken);

    /// <summary>Agrega un bloqueo.</summary>
    void Add(Block block);

    /// <summary>Quita un bloqueo.</summary>
    void Remove(Block block);
}
