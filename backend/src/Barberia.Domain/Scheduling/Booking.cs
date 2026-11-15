using System.Security.Cryptography;
using Barberia.Domain.Common;

namespace Barberia.Domain.Scheduling;

/// <summary>Turno de un cliente con un barbero para un servicio, con su enlace secreto para cambiarlo.</summary>
public sealed class Booking : Entity
{
    private const int TokenBytes = 24;

    /// <summary>Constructor para la base de datos.</summary>
    private Booking()
    {
    }

    /// <summary>Número correlativo que ve el cliente en su ticket.</summary>
    public int Number { get; private init; }

    /// <summary>Barbero que atiende.</summary>
    public Guid BarberId { get; private set; }

    /// <summary>Servicio reservado.</summary>
    public Guid ServiceId { get; private init; }

    /// <summary>Nombre del servicio al momento de reservar.</summary>
    public string ServiceName { get; private init; } = string.Empty;

    /// <summary>Precio en centavos al momento de reservar.</summary>
    public int PriceCents { get; private init; }

    /// <summary>Horario del turno en UTC.</summary>
    public TimeRange Range { get; private set; } = null!;

    /// <summary>Quién viene.</summary>
    public Customer Customer { get; private init; } = null!;

    /// <summary>Estado del turno.</summary>
    public BookingStatus Status { get; private set; } = BookingStatus.Booked;

    /// <summary>Indica si lo anotó la barbería por alguien que llegó sin reserva.</summary>
    public bool IsWalkIn { get; private init; }

    /// <summary>Clave secreta del enlace con el que el cliente ve, cambia o cancela su turno.</summary>
    public string ManageToken { get; private init; } = string.Empty;

    /// <summary>Cuándo se creó el turno.</summary>
    public DateTimeOffset CreatedAt { get; private init; }

    /// <summary>Cuándo se le mandó el recordatorio al cliente; vacío si todavía no.</summary>
    public DateTimeOffset? RemindedAt { get; private set; }

    /// <summary>Indica si el turno ocupa la agenda: reservado o atendido.</summary>
    public bool BlocksAgenda => Status is BookingStatus.Booked or BookingStatus.Attended;

    /// <summary>Crea un turno reservado; las reglas de disponibilidad se verifican antes, en el calendario.</summary>
    public static Booking Create(
        Guid barberId,
        Guid serviceId,
        string serviceName,
        int priceCents,
        TimeRange range,
        Customer customer,
        bool isWalkIn,
        DateTimeOffset now)
    {
        return new Booking
        {
            BarberId = barberId,
            ServiceId = serviceId,
            ServiceName = serviceName,
            PriceCents = priceCents,
            Range = range,
            Customer = customer,
            IsWalkIn = isWalkIn,
            ManageToken = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(TokenBytes)),
            CreatedAt = now.ToUniversalTime(),
        };
    }

    /// <summary>Indica si el cliente todavía puede cambiar o cancelar por su cuenta.</summary>
    public Result EnsureCustomerCanChange(DateTimeOffset now, int cancelLimitMinutes)
    {
        if (Status != BookingStatus.Booked)
        {
            return SchedulingErrors.NotActive;
        }

        if (now.AddMinutes(cancelLimitMinutes) > Range.Start)
        {
            return SchedulingErrors.TooLateToChange;
        }

        return Result.Success();
    }

    /// <summary>Cancela el turno si sigue reservado.</summary>
    public Result Cancel()
    {
        if (Status != BookingStatus.Booked)
        {
            return SchedulingErrors.NotActive;
        }

        Status = BookingStatus.Cancelled;
        return Result.Success();
    }

    /// <summary>Mueve el turno a otro horario y, si cambia, a otro barbero.</summary>
    public Result Reschedule(Guid barberId, TimeRange range)
    {
        if (Status != BookingStatus.Booked)
        {
            return SchedulingErrors.NotActive;
        }

        BarberId = barberId;
        Range = range;
        RemindedAt = null;
        return Result.Success();
    }

    /// <summary>Marca cómo terminó el turno: atendido, no vino, o de vuelta a reservado si fue un error.</summary>
    public Result SetOutcome(BookingStatus status)
    {
        if (Status == BookingStatus.Cancelled || status == BookingStatus.Cancelled)
        {
            return SchedulingErrors.NotActive;
        }

        Status = status;
        return Result.Success();
    }

    /// <summary>Indica si toca recordarle el turno al cliente: sigue reservado, no llegó sin turno y aún no se le avisó.</summary>
    public bool NeedsReminder => Status == BookingStatus.Booked && !IsWalkIn && RemindedAt is null;

    /// <summary>Anota que ya se mandó el recordatorio para no repetirlo.</summary>
    public void MarkReminded(DateTimeOffset now)
    {
        RemindedAt = now.ToUniversalTime();
    }
}
