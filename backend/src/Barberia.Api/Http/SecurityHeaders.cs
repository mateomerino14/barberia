using Microsoft.AspNetCore.HttpOverrides;

namespace Barberia.Api.Http;

/// <summary>Cabeceras de seguridad y lectura de la IP real detrás del proxy del hosting.</summary>
public static class SecurityHeaders
{
    /// <summary>Configura cuánto confiar en las cabeceras X-Forwarded-* según Proxy:TrustForwardedHeaders.</summary>
    public static IServiceCollection AddProxySupport(this IServiceCollection services, IConfiguration configuration)
    {
        bool trust = configuration.GetValue<bool>("Proxy:TrustForwardedHeaders");
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            if (!trust)
            {
                options.ForwardedHeaders = ForwardedHeaders.None;
                return;
            }

            // Solo se toma el último salto, el que agrega el proxy del hosting; lo que mande el cliente se ignora.
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });
        return services;
    }

    /// <summary>Agrega a cada respuesta las cabeceras que cierran el paso a usos indebidos del navegador.</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            IHeaderDictionary headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
            headers.CacheControl = "no-store";
            await next(context);
        });
    }
}
