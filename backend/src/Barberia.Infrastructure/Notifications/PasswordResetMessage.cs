using System.Globalization;
using System.Net;
using Barberia.Application.Ports;
using Barberia.Domain.Access;
using Barberia.Domain.Common;

namespace Barberia.Infrastructure.Notifications;

/// <summary>Texto del correo con el enlace para crear una contraseña nueva, en español y con la barbería como remitente.</summary>
public sealed record PasswordResetMessage(string SenderName, string Subject, string Text, string Html)
{
    private const string DefaultSender = "Barbería";
    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-BO");

    /// <summary>Arma el mensaje con el enlace y la hora en que vence.</summary>
    public static PasswordResetMessage For(PasswordResetNotice notice, string publicWebUrl)
    {
        string sender = DefaultSender;
        if (!string.IsNullOrWhiteSpace(notice.ShopName))
        {
            sender = notice.ShopName.Trim();
        }

        string link = $"{publicWebUrl.TrimEnd('/')}/restablecer/{notice.Token}";
        string until = BusinessTime.ToLocal(notice.ExpiresAt).ToString("HH:mm", Spanish);
        string subject = $"Crea una contraseña nueva para el panel de {sender}";
        string lead = $"Alguien pidió cambiar la contraseña de esta cuenta. El enlace sirve una sola vez y vence en {PasswordReset.LifetimeMinutes} minutos, a las {until}.";
        string ignore = "Si no fuiste tú, ignora este correo: tu contraseña no cambia.";
        string text = $"{lead}\n\nCrear una contraseña nueva: {link}\n\n{ignore}";
        string html = $"<p>{Encode(lead)}</p><p><a href=\"{Encode(link)}\">Crear una contraseña nueva</a></p><p>{Encode(ignore)}</p>";
        return new PasswordResetMessage(sender, subject, text, html);
    }

    /// <summary>Escapa el texto para ponerlo dentro del HTML del correo.</summary>
    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }
}
