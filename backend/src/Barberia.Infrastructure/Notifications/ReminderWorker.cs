using Barberia.Application.Features.Bookings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Barberia.Infrastructure.Notifications;

/// <summary>Tarea de fondo que cada pocos minutos manda los recordatorios pendientes.</summary>
public sealed partial class ReminderWorker(IServiceScopeFactory scopes, IOptions<ReminderOptions> options, ILogger<ReminderWorker> logger) : BackgroundService
{
    /// <summary>Revisa al arrancar y luego cada intervalo, hasta que la API se apague.</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ReminderOptions settings = options.Value;
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(settings.IntervalMinutes));
        do
        {
            await RunOnceAsync(settings.HoursBefore, stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    /// <summary>Manda una tanda; si algo falla lo registra y espera la próxima vuelta.</summary>
    private async Task RunOnceAsync(int hoursBefore, CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = scopes.CreateScope();
            SendRemindersHandler handler = scope.ServiceProvider.GetRequiredService<SendRemindersHandler>();
            int sent = await handler.HandleAsync(hoursBefore, stoppingToken);
            if (sent > 0)
            {
                LogSent(logger, sent);
            }
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogFailed(logger, exception);
        }
    }

    /// <summary>Registra cuántos recordatorios se mandaron.</summary>
    [LoggerMessage(Level = LogLevel.Information, Message = "Se mandaron {Count} recordatorios")]
    private static partial void LogSent(ILogger logger, int count);

    /// <summary>Registra que la tanda falló.</summary>
    [LoggerMessage(Level = LogLevel.Warning, Message = "No se pudieron mandar los recordatorios")]
    private static partial void LogFailed(ILogger logger, Exception exception);
}
