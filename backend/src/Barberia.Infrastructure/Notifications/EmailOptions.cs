namespace Barberia.Infrastructure.Notifications;

/// <summary>Configuración del correo de avisos, leída de la sección "Email".</summary>
public sealed class EmailOptions
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string Section = "Email";

    /// <summary>Clave de la API de Brevo; vacía apaga los correos.</summary>
    public string BrevoApiKey { get; init; } = string.Empty;

    /// <summary>Correo remitente verificado en Brevo.</summary>
    public string SenderEmail { get; init; } = string.Empty;

    /// <summary>Dirección pública de la web, para armar el enlace del turno.</summary>
    public string PublicWebUrl { get; init; } = string.Empty;
}
