using Barberia.Application.Ports;
using Microsoft.Extensions.Options;

namespace Barberia.Infrastructure.Notifications;

/// <summary>Adaptador que manda por correo, con Brevo, el enlace para crear una contraseña nueva.</summary>
public sealed class BrevoAccessNotifier(BrevoMailer mailer, IOptions<EmailOptions> options) : IAccessNotifier
{
    /// <summary>Con Brevo configurado, el correo siempre está disponible.</summary>
    public bool IsAvailable => true;

    /// <summary>Manda el enlace a la cuenta que lo pidió.</summary>
    public Task SendPasswordResetAsync(PasswordResetNotice notice, CancellationToken cancellationToken)
    {
        PasswordResetMessage message = PasswordResetMessage.For(notice, options.Value.PublicWebUrl);
        OutgoingEmail outgoing = new(notice.Email, notice.Email, message.SenderName, message.Subject, message.Text, message.Html);
        return mailer.SendAsync(outgoing, cancellationToken);
    }
}

/// <summary>Sin correos configurados no se puede recuperar la contraseña por correo; la web no ofrece la opción.</summary>
public sealed class NoAccessNotifier : IAccessNotifier
{
    /// <summary>Nunca está disponible.</summary>
    public bool IsAvailable => false;

    /// <summary>No manda nada.</summary>
    public Task SendPasswordResetAsync(PasswordResetNotice notice, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
