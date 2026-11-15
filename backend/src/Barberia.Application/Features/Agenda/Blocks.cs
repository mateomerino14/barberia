using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;

namespace Barberia.Application.Features.Agenda;

/// <summary>Horas que un barbero no atiende, un día dado.</summary>
public sealed record AddBlockCommand(Guid BarberId, DateOnly Date, TimeOnly From, TimeOnly To, string? Reason);

/// <summary>Bloquea o desbloquea horas en la agenda de un barbero.</summary>
public sealed class BlockHandler(IBlockRepository blocks, IUnitOfWork unitOfWork)
{
    /// <summary>Agrega un bloqueo; los turnos ya reservados en esas horas no se tocan.</summary>
    public async Task<Result<Block>> AddAsync(AddBlockCommand command, StaffActor actor, CancellationToken cancellationToken)
    {
        if (!actor.CanManage(command.BarberId))
        {
            return StaffActor.OtherBarber;
        }

        Result<TimeRange> range = TimeRange.Create(BusinessTime.At(command.Date, command.From), BusinessTime.At(command.Date, command.To));
        if (range.IsFailure)
        {
            return range.Error!;
        }

        Result<Block> block = Block.Create(command.BarberId, range.Value, command.Reason);
        if (block.IsFailure)
        {
            return block;
        }

        blocks.Add(block.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return block;
    }

    /// <summary>Quita un bloqueo.</summary>
    public async Task<Result> RemoveAsync(Guid id, StaffActor actor, CancellationToken cancellationToken)
    {
        Block? block = await blocks.GetAsync(id, cancellationToken);
        if (block is null)
        {
            return SchedulingErrors.BlockNotFound;
        }

        if (!actor.CanManage(block.BarberId))
        {
            return StaffActor.OtherBarber;
        }

        blocks.Remove(block);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
