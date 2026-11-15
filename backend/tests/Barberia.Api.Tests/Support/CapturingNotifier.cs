using System.Collections.Concurrent;
using Barberia.Application.Ports;

namespace Barberia.Api.Tests.Support;

/// <summary>Canal de avisos de prueba que guarda lo que se habría enviado.</summary>
public sealed class CapturingNotifier : INotifier
{
    /// <summary>Avisos recibidos, en orden.</summary>
    public ConcurrentQueue<BookingNotice> Notices { get; } = new();

    /// <summary>Guarda el aviso.</summary>
    public Task NotifyAsync(BookingNotice notice, CancellationToken cancellationToken)
    {
        Notices.Enqueue(notice);
        return Task.CompletedTask;
    }
}
