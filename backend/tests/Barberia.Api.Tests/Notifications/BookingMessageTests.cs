using Barberia.Application.Ports;
using Barberia.Domain.Scheduling;
using Barberia.Infrastructure.Notifications;

namespace Barberia.Api.Tests.Notifications;

/// <summary>Pruebas del correo al cliente: remitente, asunto y enlace.</summary>
public sealed class BookingMessageTests
{
    private const string ShopName = "Barbería Don Lucho";
    private const string WebUrl = "https://barberia.test/";
    private const int ServiceMinutes = 30;
    private const int PriceCents = 4000;
    private static readonly DateTimeOffset Start = new(2026, 11, 17, 14, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Now = Start.AddDays(-1);

    /// <summary>El correo sale con el nombre de la barbería como remitente y el enlace del turno.</summary>
    [Fact]
    public void For_UsesShopNameAsSender()
    {
        Booking booking = NewBooking();

        BookingMessage message = BookingMessage.For(new BookingNotice(BookingEvent.Reminder, booking, $"  {ShopName} ", "Sopocachi", "Lucho"), WebUrl);

        Assert.Equal(ShopName, message.SenderName);
        Assert.StartsWith("Recordatorio: turno", message.Subject, StringComparison.Ordinal);
        Assert.Contains($"https://barberia.test/turno/{booking.ManageToken}", message.Text, StringComparison.Ordinal);
    }

    /// <summary>Sin nombre de barbería, el remitente es "Barbería".</summary>
    [Fact]
    public void For_WithoutShopName_UsesDefaultSender()
    {
        BookingMessage message = BookingMessage.For(new BookingNotice(BookingEvent.Confirmed, NewBooking(), "", "", "Lucho"), WebUrl);

        Assert.Equal("Barbería", message.SenderName);
    }

    /// <summary>Arma un turno de prueba.</summary>
    private static Booking NewBooking()
    {
        Customer customer = Customer.Create("Diego", "71234567", "diego@correo.bo", true).Value;
        return Booking.Create(Guid.NewGuid(), Guid.NewGuid(), "Corte clásico", PriceCents, TimeRange.FromStart(Start, ServiceMinutes), customer, false, Now);
    }
}
