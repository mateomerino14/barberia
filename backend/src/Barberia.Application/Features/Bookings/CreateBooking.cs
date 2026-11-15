using Barberia.Application.Ports;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Bookings;

/// <summary>Reserva que hace un cliente desde la página pública.</summary>
public sealed record CreateBookingCommand(
    Guid ServiceId,
    Guid? BarberId,
    DateTimeOffset Start,
    string? CustomerName,
    string? CustomerPhone,
    string? CustomerEmail);

/// <summary>Reserva un turno si la hora sigue libre; con "cualquiera" elige al barbero con menos turnos ese día.</summary>
public sealed class CreateBookingHandler(
    IShopRepository shops,
    IServiceRepository services,
    IBarberRepository barbers,
    IBookingRepository bookings,
    ScheduleReader schedule,
    IUnitOfWork unitOfWork,
    IClock clock,
    BookingNotifications notifications)
{
    /// <summary>Crea el turno o explica por qué no se pudo.</summary>
    public async Task<Result<Booking>> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        Service? service = await services.GetAsync(command.ServiceId, cancellationToken);
        if (service is null || !service.IsActive)
        {
            return SchedulingErrors.ServiceUnavailable;
        }

        Result<Customer> customer = Customer.Create(command.CustomerName, command.CustomerPhone, command.CustomerEmail, true);
        if (customer.IsFailure)
        {
            return customer.Error!;
        }

        Barber? barber = await PickBarberAsync(shop, service, command, cancellationToken);
        if (barber is null)
        {
            return SchedulingErrors.SlotUnavailable;
        }

        TimeRange range = TimeRange.FromStart(command.Start, service.DurationMinutes);
        Booking booking = Booking.Create(barber.Id, service.Id, service.Name, service.Price.Cents, range, customer.Value, false, clock.Now);
        bookings.Add(booking);
        Result saved = await unitOfWork.SaveScheduleChangesAsync(cancellationToken);
        if (saved.IsFailure)
        {
            return saved.Error!;
        }

        await notifications.SendAsync(BookingEvent.Confirmed, booking, cancellationToken);
        return booking;
    }

    /// <summary>Devuelve el barbero que puede atender a esa hora, o null si nadie puede.</summary>
    private async Task<Barber?> PickBarberAsync(Shop shop, Service service, CreateBookingCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<Barber> active = await barbers.ListAsync(true, cancellationToken);
        DateOnly date = BusinessTime.DateOf(command.Start);
        Barber? chosen = null;
        int fewest = int.MaxValue;
        foreach (Barber barber in active.Where(item => item.Offers(service.Id) && (command.BarberId is null || item.Id == command.BarberId)))
        {
            IReadOnlyList<TimeRange> busy = await schedule.BusyAsync(barber.Id, date, null, cancellationToken);
            if (!AvailabilityCalculator.IsFree(shop, barber, command.Start, service.DurationMinutes, busy, clock.Now))
            {
                continue;
            }

            int count = await schedule.CountActiveAsync(barber.Id, date, cancellationToken);
            if (count < fewest)
            {
                chosen = barber;
                fewest = count;
            }
        }

        return chosen;
    }
}
