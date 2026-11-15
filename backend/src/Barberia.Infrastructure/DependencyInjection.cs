using Barberia.Application.Ports;
using Barberia.Infrastructure.Time;
using Microsoft.Extensions.DependencyInjection;

namespace Barberia.Infrastructure;

/// <summary>Registra los adaptadores de salida en el contenedor de dependencias.</summary>
public static class DependencyInjection
{
    /// <summary>Agrega los adaptadores de infraestructura a los servicios de la aplicación.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();
        return services;
    }
}
