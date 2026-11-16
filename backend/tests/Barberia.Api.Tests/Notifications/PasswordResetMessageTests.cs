using Barberia.Application.Ports;
using Barberia.Infrastructure.Notifications;

namespace Barberia.Api.Tests.Notifications;

/// <summary>Pruebas del correo para crear una contraseña nueva.</summary>
public sealed class PasswordResetMessageTests
{
    private const string Token = "clave-de-prueba";
    private static readonly DateTimeOffset ExpiresAt = new(2026, 11, 17, 14, 30, 0, TimeSpan.Zero);

    /// <summary>El correo sale de parte de la barbería, con el enlace y la hora de La Paz en que vence.</summary>
    [Fact]
    public void For_HasLinkSenderAndExpiry()
    {
        PasswordResetMessage message = PasswordResetMessage.For(new PasswordResetNotice("dueno@barberia.bo", "Barbería Don Lucho", Token, ExpiresAt), "https://barberia.test/");

        Assert.Equal("Barbería Don Lucho", message.SenderName);
        Assert.Contains("https://barberia.test/restablecer/clave-de-prueba", message.Text, StringComparison.Ordinal);
        Assert.Contains("a las 10:30", message.Text, StringComparison.Ordinal);
        Assert.Contains("Si no fuiste tú", message.Text, StringComparison.Ordinal);
        Assert.Contains("restablecer/clave-de-prueba", message.Html, StringComparison.Ordinal);
    }
}
