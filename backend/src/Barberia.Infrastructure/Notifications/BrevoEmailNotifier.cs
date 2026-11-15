using System.Net.Http.Json;
using Barberia.Application.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Barberia.Infrastructure.Notifications;

/// <summary>Adaptador que avisa por correo con la API de Brevo; si falla, lo registra y sigue.</summary>
public sealed partial class BrevoEmailNotifier(HttpClient http, IOptions<EmailOptions> options, ILogger<BrevoEmailNotifier> logger) : INotifier
{
    /// <summary>Dirección de la API de correos transaccionales de Brevo.</summary>
    public const string Endpoint = "https://api.brevo.com/v3/smtp/email";

    private const string ApiKeyHeader = "api-key";

    /// <summary>Envía el correo si el cliente dejó uno.</summary>
    public async Task NotifyAsync(BookingNotice notice, CancellationToken cancellationToken)
    {
        string email = notice.Booking.Customer.Email;
        if (string.IsNullOrEmpty(email))
        {
            return;
        }

        EmailOptions settings = options.Value;
        BookingMessage message = BookingMessage.For(notice, settings.PublicWebUrl);
        object body = new
        {
            sender = new { name = message.SenderName, email = settings.SenderEmail },
            to = new[] { new { email, name = notice.Booking.Customer.Name } },
            subject = message.Subject,
            textContent = message.Text,
            htmlContent = message.Html,
        };
        using HttpRequestMessage request = new(HttpMethod.Post, Endpoint) { Content = JsonContent.Create(body) };
        request.Headers.Add(ApiKeyHeader, settings.BrevoApiKey);
        try
        {
            using HttpResponseMessage response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                LogRejected(logger, notice.Booking.Number, (int)response.StatusCode);
            }
        }
        catch (HttpRequestException exception)
        {
            LogFailed(logger, notice.Booking.Number, exception);
        }
    }

    /// <summary>Registra que Brevo rechazó el correo.</summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "Brevo rechazó el aviso del turno {Number} con el estado {Status}")]
    private static partial void LogRejected(ILogger logger, int number, int status);

    /// <summary>Registra que no se pudo hablar con Brevo.</summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "No se pudo enviar el aviso del turno {Number}")]
    private static partial void LogFailed(ILogger logger, int number, Exception exception);
}
