using Barberia.Application.Ports;
using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Bookings;

/// <summary>Turno con los datos que ve el cliente en su enlace.</summary>
public sealed record BookingDetails(Booking Booking, string BarberName, int CancelLimitMinutes);

/// <summary>Muestra un turno a partir de la clave de su enlace.</summary>
public sealed class GetBookingHandler(IBookingRepository bookings, IBarberRepository barbers, IShopRepository shops)
{
    /// <summary>Devuelve el turno con el nombre del barbero y el plazo para cambiarlo.</summary>
    public async Task<Result<BookingDetails>> HandleAsync(string token, CancellationToken cancellationToken)
    {
        Booking? booking = await bookings.FindByTokenAsync(token, cancellationToken);
        if (booking is null)
        {
            return SchedulingErrors.NotFound;
        }

        Barber? barber = await barbers.GetAsync(booking.BarberId, cancellationToken);
        Shop? shop = await shops.GetAsync(cancellationToken);
        int limit = BookingRules.Default.CancelLimitMinutes;
        if (shop is not null)
        {
            limit = shop.Rules.CancelLimitMinutes;
        }

        return new BookingDetails(booking, barber?.Name ?? string.Empty, limit);
    }
}

/// <summary>Cancela un turno desde el enlace del cliente, dentro del plazo.</summary>
public sealed class CancelBookingHandler(IBookingRepository bookings, IShopRepository shops, IUnitOfWork unitOfWork, IClock clock)
{
    /// <summary>Cancela el turno si todavía se puede.</summary>
    public async Task<Result<Booking>> HandleAsync(string token, CancellationToken cancellationToken)
    {
        Booking? booking = await bookings.FindByTokenAsync(token, cancellationToken);
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (booking is null || shop is null)
        {
            return SchedulingErrors.NotFound;
        }

        Result allowed = booking.EnsureCustomerCanChange(clock.Now, shop.Rules.CancelLimitMinutes);
        if (allowed.IsFailure)
        {
            return allowed.Error!;
        }

        booking.Cancel();
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return booking;
    }
}

/// <summary>Nueva hora (y opcionalmente barbero) para un turno.</summary>
public sealed record RescheduleBookingCommand(DateTimeOffset Start, Guid? BarberId);

/// <summary>Cambia la hora de un turno desde el enlace del cliente, dentro del plazo y si la hora nueva está libre.</summary>
public sealed class RescheduleBookingHandler(
    IBookingRepository bookings,
    IShopRepository shops,
    IServiceRepository services,
    IBarberRepository barbers,
    ScheduleReader schedule,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    /// <summary>Mueve el turno o explica por qué no se pudo.</summary>
    public async Task<Result<Booking>> HandleAsync(string token, RescheduleBookingCommand command, CancellationToken cancellationToken)
    {
        Booking? booking = await bookings.FindByTokenAsync(token, cancellationToken);
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (booking is null || shop is null)
        {
            return SchedulingErrors.NotFound;
        }

        Result allowed = booking.EnsureCustomerCanChange(clock.Now, shop.Rules.CancelLimitMinutes);
        if (allowed.IsFailure)
        {
            return allowed.Error!;
        }

        Service? service = await services.GetAsync(booking.ServiceId, cancellationToken);
        Barber? barber = await barbers.GetAsync(command.BarberId ?? booking.BarberId, cancellationToken);
        if (service is null || barber is null || !barber.Offers(service.Id))
        {
            return SchedulingErrors.BarberUnavailable;
        }

        int minutes = (int)(booking.Range.End - booking.Range.Start).TotalMinutes;
        IReadOnlyList<TimeRange> busy = await schedule.BusyAsync(barber.Id, BusinessTime.DateOf(command.Start), booking.Id, cancellationToken);
        if (!AvailabilityCalculator.IsFree(shop, barber, command.Start, minutes, busy, clock.Now))
        {
            return SchedulingErrors.SlotUnavailable;
        }

        booking.Reschedule(barber.Id, TimeRange.FromStart(command.Start, minutes));
        Result saved = await unitOfWork.SaveScheduleChangesAsync(cancellationToken);
        if (saved.IsFailure)
        {
            return saved.Error!;
        }

        return booking;
    }
}
