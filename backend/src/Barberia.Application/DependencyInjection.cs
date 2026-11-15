using Microsoft.Extensions.DependencyInjection;

namespace Barberia.Application;

/// <summary>Registra los casos de uso en el contenedor de dependencias.</summary>
public static class DependencyInjection
{
    private const string HandlerSuffix = "Handler";

    /// <summary>Agrega cada manejador de caso de uso como servicio con alcance de petición.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        IEnumerable<Type> handlers = typeof(DependencyInjection).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && type.Name.EndsWith(HandlerSuffix, StringComparison.Ordinal));
        foreach (Type handler in handlers)
        {
            services.AddScoped(handler);
        }

        return services;
    }
}
