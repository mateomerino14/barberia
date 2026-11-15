using Barberia.Application.Features.Bookings;
using Barberia.Application.Ports;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Agenda;

/// <summary>Turno que anota la barbería: por teléfono o por alguien que llegó sin reserva.</summary>
public sealed record CreateStaffBookingCommand(
    Guid ServiceId,
    Guid BarberId,
    DateTimeOffset Start,
    string? CustomerName,
    string? CustomerPhone,
    bool IsWalkIn);

/// <summary>Anota un turno desde el panel; no pide anticipación, pero sí que quepa en el día del barbero.</summary>
public sealed class CreateStaffBookingHandler(
    IShopRepository shops,
    IServiceRepository services,
    IBarberRepository barbers,
    IBookingRepository bookings,
    ScheduleReader schedule,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    /// <summary>Crea el turno si el barbero lo puede atender a esa hora.</summary>
    public async Task<Result<Booking>> HandleAsync(CreateStaffBookingCommand command, StaffActor actor, CancellationToken cancellationToken)
    {
        if (!actor.CanManage(command.BarberId))
        {
            return StaffActor.OtherBarber;
        }

        Shop? shop = await shops.GetAsync(cancellationToken);
        Service? service = await services.GetAsync(command.ServiceId, cancellationToken);
        Barber? barber = await barbers.GetAsync(command.BarberId, cancellationToken);
        if (shop is null || service is null || barber is null || !barber.Offers(service.Id))
        {
            return SchedulingErrors.BarberUnavailable;
        }

        Result<Customer> customer = Customer.Create(command.CustomerName, command.CustomerPhone, null, !command.IsWalkIn);
        if (customer.IsFailure)
        {
            return customer.Error!;
        }

        TimeRange range = TimeRange.FromStart(command.Start, service.DurationMinutes);
        IReadOnlyList<TimeRange> busy = await schedule.BusyAsync(barber.Id, BusinessTime.DateOf(command.Start), null, cancellationToken);
        if (!AvailabilityCalculator.FitsWorkday(shop, barber, range, busy))
        {
            return SchedulingErrors.SlotUnavailable;
        }

        Booking booking = Booking.Create(barber.Id, service.Id, service.Name, service.Price.Cents, range, customer.Value, command.IsWalkIn, clock.Now);
        bookings.Add(booking);
        Result saved = await unitOfWork.SaveScheduleChangesAsync(cancellationToken);
        if (saved.IsFailure)
        {
            return saved.Error!;
        }

        return booking;
    }
}

/// <summary>Marca cómo terminó un turno o lo cancela desde el panel, sin el plazo que tiene el cliente.</summary>
public sealed class UpdateStaffBookingHandler(IBookingRepository bookings, IUnitOfWork unitOfWork, BookingNotifications notifications)
{
    /// <summary>Cambia el estado: atendido, no vino o de vuelta a reservado.</summary>
    public async Task<Result<Booking>> SetStatusAsync(Guid id, BookingStatus status, StaffActor actor, CancellationToken cancellationToken)
    {
        Result<Booking> booking = await FindAsync(id, actor, cancellationToken);
        if (booking.IsFailure)
        {
            return booking;
        }

        Result result = booking.Value.SetOutcome(status);
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return booking;
    }

    /// <summary>Cancela el turno y le avisa al cliente.</summary>
    public async Task<Result<Booking>> CancelAsync(Guid id, StaffActor actor, CancellationToken cancellationToken)
    {
        Result<Booking> booking = await FindAsync(id, actor, cancellationToken);
        if (booking.IsFailure)
        {
            return booking;
        }

        Result result = booking.Value.Cancel();
        if (result.IsFailure)
        {
            return result.Error!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await notifications.SendAsync(BookingEvent.Cancelled, booking.Value, cancellationToken);
        return booking;
    }

    /// <summary>Busca el turno y verifica que quien lo pide lo pueda manejar.</summary>
    private async Task<Result<Booking>> FindAsync(Guid id, StaffActor actor, CancellationToken cancellationToken)
    {
        Booking? booking = await bookings.GetAsync(id, cancellationToken);
        if (booking is null)
        {
            return SchedulingErrors.NotFound;
        }

        if (!actor.CanManage(booking.BarberId))
        {
            return StaffActor.OtherBarber;
        }

        return booking;
    }
}
