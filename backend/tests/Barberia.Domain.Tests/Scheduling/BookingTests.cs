using Barberia.Domain.Scheduling;

namespace Barberia.Domain.Tests.Scheduling;

/// <summary>Pruebas del turno, sus cambios y los datos del cliente.</summary>
public sealed class BookingTests
{
    private const int FortyFive = 45;
    private const int TwoHours = 120;
    private const int Price = 5000;
    private static readonly DateTimeOffset Start = new(2026, 11, 14, 14, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ThreeHoursBefore = Start.AddHours(-3);
    private static readonly DateTimeOffset OneHourBefore = Start.AddHours(-1);

    /// <summary>Dos intervalos que solo se tocan en el borde no chocan.</summary>
    [Fact]
    public void TimeRange_Touching_DoesNotOverlap()
    {
        TimeRange first = TimeRange.FromStart(Start, FortyFive);
        TimeRange next = TimeRange.FromStart(first.End, FortyFive);
        TimeRange inside = TimeRange.FromStart(Start.AddMinutes(AvailabilityCalculator.SlotStepMinutes), FortyFive);

        Assert.False(first.Overlaps(next));
        Assert.True(first.Overlaps(inside));
    }

    /// <summary>El cliente puede cambiar antes del plazo, pero no después.</summary>
    [Fact]
    public void EnsureCustomerCanChange_BeforeAndAfterLimit()
    {
        Booking booking = NewBooking();

        Assert.True(booking.EnsureCustomerCanChange(ThreeHoursBefore, TwoHours).IsSuccess);
        Assert.Equal(SchedulingErrors.TooLateToChange, booking.EnsureCustomerCanChange(OneHourBefore, TwoHours).Error);
    }

    /// <summary>Tras el recordatorio no hace falta otro, pero si cambia la hora se vuelve a recordar.</summary>
    [Fact]
    public void Reminder_AfterRescheduleIsNeededAgain()
    {
        Booking booking = NewBooking();

        booking.MarkReminded(ThreeHoursBefore);
        bool afterReminder = booking.NeedsReminder;
        booking.Reschedule(booking.BarberId, TimeRange.FromStart(Start.AddHours(1), FortyFive));

        Assert.False(afterReminder);
        Assert.True(booking.NeedsReminder);
    }

    /// <summary>Un turno cancelado deja la agenda libre y no se puede cancelar ni mover otra vez.</summary>
    [Fact]
    public void Cancel_Twice_SecondFails()
    {
        Booking booking = NewBooking();

        Assert.True(booking.Cancel().IsSuccess);
        Assert.False(booking.BlocksAgenda);
        Assert.Equal(SchedulingErrors.NotActive, booking.Cancel().Error);
        Assert.Equal(SchedulingErrors.NotActive, booking.Reschedule(Guid.NewGuid(), TimeRange.FromStart(Start, FortyFive)).Error);
    }

    /// <summary>Atendido sigue ocupando la agenda; no vino la libera; un error se puede deshacer.</summary>
    [Fact]
    public void SetOutcome_ChangesWhatBlocksTheAgenda()
    {
        Booking booking = NewBooking();

        booking.SetOutcome(BookingStatus.Attended);
        Assert.True(booking.BlocksAgenda);
        booking.SetOutcome(BookingStatus.NoShow);
        Assert.False(booking.BlocksAgenda);
        booking.SetOutcome(BookingStatus.Booked);
        Assert.True(booking.BlocksAgenda);
        Assert.True(booking.SetOutcome(BookingStatus.Cancelled).IsFailure);
    }

    /// <summary>Cada turno tiene su propia clave secreta.</summary>
    [Fact]
    public void Create_TwoBookings_HaveDifferentTokens()
    {
        Assert.NotEqual(NewBooking().ManageToken, NewBooking().ManageToken);
    }

    /// <summary>El celular se guarda solo con dígitos; uno corto se rechaza; sin reserva puede faltar.</summary>
    [Fact]
    public void Customer_Phone_Rules()
    {
        Assert.Equal("71234567", Customer.Create("Diego", "7 123-4567", null, true).Value.Phone);
        Assert.Equal(SchedulingErrors.InvalidPhone, Customer.Create("Diego", "123", null, true).Error);
        Assert.Equal(SchedulingErrors.InvalidPhone, Customer.Create("Diego", "", null, true).Error);
        Assert.True(Customer.Create("Diego", "", null, false).IsSuccess);
        Assert.True(Customer.Create("Diego", "71234567", "correo-malo", true).IsFailure);
    }

    /// <summary>Crea un turno de prueba.</summary>
    private static Booking NewBooking()
    {
        Customer customer = Customer.Create("Diego", "71234567", null, true).Value;
        return Booking.Create(Guid.NewGuid(), Guid.NewGuid(), "Degradado", Price, TimeRange.FromStart(Start, FortyFive), customer, false, ThreeHoursBefore);
    }
}
