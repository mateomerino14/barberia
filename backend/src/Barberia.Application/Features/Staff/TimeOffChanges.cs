using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Staff;

/// <summary>Ausencia nueva de un barbero.</summary>
public sealed record AddTimeOffCommand(DateOnly From, DateOnly To, string? Reason);

/// <summary>Registra una ausencia de un barbero.</summary>
public sealed class AddTimeOffHandler(IBarberRepository barbers, IUnitOfWork unitOfWork)
{
    /// <summary>Guarda la ausencia si las fechas son válidas.</summary>
    public async Task<Result<Barber>> HandleAsync(Guid barberId, AddTimeOffCommand command, CancellationToken cancellationToken)
    {
        Barber? barber = await barbers.GetAsync(barberId, cancellationToken);
        if (barber is null)
        {
            return StaffErrors.NotFound;
        }

        Result<TimeOff> timeOff = barber.AddTimeOff(command.From, command.To, command.Reason);
        if (timeOff.IsFailure)
        {
            return timeOff.Error!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return barber;
    }
}

/// <summary>Quita una ausencia de un barbero.</summary>
public sealed class RemoveTimeOffHandler(IBarberRepository barbers, IUnitOfWork unitOfWork)
{
    /// <summary>Quita la ausencia si existe.</summary>
    public async Task<Result<Barber>> HandleAsync(Guid barberId, Guid timeOffId, CancellationToken cancellationToken)
    {
        Barber? barber = await barbers.GetAsync(barberId, cancellationToken);
        if (barber is null)
        {
            return StaffErrors.NotFound;
        }

        Result result = barber.RemoveTimeOff(timeOffId);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return barber;
    }
}
