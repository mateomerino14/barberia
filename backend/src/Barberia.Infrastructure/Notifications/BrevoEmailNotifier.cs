using Barberia.Application.Ports;
using Microsoft.Extensions.Options;

namespace Barberia.Infrastructure.Notifications;

/// <summary>Adaptador que avisa al cliente sobre su turno por correo, con Brevo.</summary>
public sealed class BrevoEmailNotifier(BrevoMailer mailer, IOptions<EmailOptions> options) : INotifier
{
    /// <summary>Envía el correo si el cliente dejó uno.</summary>
    public Task NotifyAsync(BookingNotice notice, CancellationToken cancellationToken)
    {
        string email = notice.Booking.Customer.Email;
        if (string.IsNullOrEmpty(email))
        {
            return Task.CompletedTask;
        }

        BookingMessage message = BookingMessage.For(notice, options.Value.PublicWebUrl);
        OutgoingEmail outgoing = new(email, notice.Booking.Customer.Name, message.SenderName, message.Subject, message.Text, message.Html);
        return mailer.SendAsync(outgoing, cancellationToken);
    }
}
