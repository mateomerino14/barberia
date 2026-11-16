using System.Collections.Concurrent;
using Barberia.Application.Ports;

namespace Barberia.Api.Tests.Support;

/// <summary>Canal de correos de acceso de prueba que guarda los enlaces que se habrían enviado.</summary>
public sealed class CapturingAccessNotifier : IAccessNotifier
{
    /// <summary>En las pruebas los correos siempre están disponibles.</summary>
    public bool IsAvailable => true;

    /// <summary>Enlaces enviados, en orden.</summary>
    public ConcurrentQueue<PasswordResetNotice> Sent { get; } = new();

    /// <summary>Guarda el enlace.</summary>
    public Task SendPasswordResetAsync(PasswordResetNotice notice, CancellationToken cancellationToken)
    {
        Sent.Enqueue(notice);
        return Task.CompletedTask;
    }
}
