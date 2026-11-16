using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Barberia.Infrastructure.Notifications;

/// <summary>Correo listo para mandar: a quién, de parte de quién y su contenido.</summary>
public sealed record OutgoingEmail(string ToEmail, string ToName, string SenderName, string Subject, string Text, string Html);

/// <summary>Manda correos con la API de Brevo; si falla, lo registra y sigue, sin romper lo que el usuario estaba haciendo.</summary>
public sealed partial class BrevoMailer(HttpClient http, IOptions<EmailOptions> options, ILogger<BrevoMailer> logger)
{
    /// <summary>Dirección de la API de correos transaccionales de Brevo.</summary>
    public const string Endpoint = "https://api.brevo.com/v3/smtp/email";

    private const string ApiKeyHeader = "api-key";

    /// <summary>Manda el correo con el remitente verificado.</summary>
    public async Task SendAsync(OutgoingEmail email, CancellationToken cancellationToken)
    {
        EmailOptions settings = options.Value;
        object body = new
        {
            sender = new { name = email.SenderName, email = settings.SenderEmail },
            to = new[] { new { email = email.ToEmail, name = email.ToName } },
            subject = email.Subject,
            textContent = email.Text,
            htmlContent = email.Html,
        };
        using HttpRequestMessage request = new(HttpMethod.Post, Endpoint) { Content = JsonContent.Create(body) };
        request.Headers.Add(ApiKeyHeader, settings.BrevoApiKey);
        try
        {
            using HttpResponseMessage response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                LogRejected(logger, email.Subject, (int)response.StatusCode);
            }
        }
        catch (HttpRequestException exception)
        {
            LogFailed(logger, email.Subject, exception);
        }
    }

    /// <summary>Registra que Brevo rechazó el correo.</summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "Brevo rechazó el correo \"{Subject}\" con el estado {Status}")]
    private static partial void LogRejected(ILogger logger, string subject, int status);

    /// <summary>Registra que no se pudo hablar con Brevo.</summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "No se pudo enviar el correo \"{Subject}\"")]
    private static partial void LogFailed(ILogger logger, string subject, Exception exception);
}
