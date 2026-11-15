using Barberia.Application.Ports;
using Barberia.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Infrastructure.Persistence.Repositories;

/// <summary>Adaptador de PostgreSQL para los bloqueos de agenda.</summary>
public sealed class BlockRepository(BarberiaDbContext context) : IBlockRepository
{
    /// <summary>Busca un bloqueo por su Id.</summary>
    public Task<Block?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Blocks.FirstOrDefaultAsync(block => block.Id == id, cancellationToken);
    }

    /// <summary>Lista los bloqueos que tocan el intervalo, ordenados por hora.</summary>
    public async Task<IReadOnlyList<Block>> ListAsync(TimeRange range, Guid? barberId, CancellationToken cancellationToken)
    {
        IQueryable<Block> query = context.Blocks.Where(block => block.Range.Start < range.End && block.Range.End > range.Start);
        if (barberId is not null)
        {
            query = query.Where(block => block.BarberId == barberId);
        }

        return await query.OrderBy(block => block.Range.Start).ToListAsync(cancellationToken);
    }

    /// <summary>Agrega un bloqueo.</summary>
    public void Add(Block block)
    {
        context.Blocks.Add(block);
    }

    /// <summary>Quita un bloqueo.</summary>
    public void Remove(Block block)
    {
        context.Blocks.Remove(block);
    }
}
