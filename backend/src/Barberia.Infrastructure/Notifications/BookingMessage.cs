using System.Globalization;
using System.Net;
using Barberia.Application.Ports;
using Barberia.Domain.Common;

namespace Barberia.Infrastructure.Notifications;

/// <summary>Texto del aviso al cliente, en español y con el enlace para manejar su turno.</summary>
public sealed record BookingMessage(string Subject, string Text, string Html)
{
    private const string NumberFormat = "D4";
    private static readonly CultureInfo Spanish = CultureInfo.GetCultureInfo("es-BO");

    /// <summary>Arma el mensaje según lo que le pasó al turno.</summary>
    public static BookingMessage For(BookingNotice notice, string publicWebUrl)
    {
        DateTimeOffset start = BusinessTime.ToLocal(notice.Booking.Range.Start);
        string number = notice.Booking.Number.ToString(NumberFormat, CultureInfo.InvariantCulture);
        string when = start.ToString("dddd d 'de' MMMM 'a las' HH:mm", Spanish);
        string link = $"{publicWebUrl.TrimEnd('/')}/turno/{notice.Booking.ManageToken}";
        string detail = $"{notice.Booking.ServiceName} con {notice.BarberName}, {when}.";
        string place = $"{notice.ShopName}. {notice.ShopAddress}".Trim().TrimEnd('.');
        string subject;
        string lead;
        switch (notice.Event)
        {
            case BookingEvent.Cancelled:
                subject = $"Turno {number} cancelado";
                lead = $"Tu turno N.º {number} quedó cancelado: {detail}";
                break;
            case BookingEvent.Reminder:
                subject = $"Recordatorio: turno {number} a las {start.ToString("HH:mm", Spanish)}";
                lead = $"Te recordamos tu turno N.º {number}: {detail}";
                break;
            case BookingEvent.Rescheduled:
                subject = $"Cambiamos la hora de tu turno {number}";
                lead = $"Tu turno N.º {number} ahora es: {detail}";
                break;
            default:
                subject = $"Tu turno {number} está confirmado";
                lead = $"Tu turno N.º {number} está confirmado: {detail}";
                break;
        }

        string text = $"Hola, {notice.Booking.Customer.Name}.\n\n{lead}\n{place}.\n\nPara ver, cambiar o cancelar tu turno: {link}";
        string html = $"<p>Hola, {Encode(notice.Booking.Customer.Name)}.</p><p>{Encode(lead)}<br>{Encode(place)}.</p>"
            + $"<p><a href=\"{Encode(link)}\">Ver, cambiar o cancelar mi turno</a></p>";
        return new BookingMessage(subject, text, html);
    }

    /// <summary>Escapa el texto para ponerlo dentro del HTML del correo.</summary>
    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }
}
