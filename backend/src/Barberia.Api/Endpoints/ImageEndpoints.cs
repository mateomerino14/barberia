using Barberia.Api.Contracts;
using Barberia.Api.Http;
using Barberia.Application.Features.Media;
using Barberia.Domain.Common;
using Barberia.Domain.Media;

namespace Barberia.Api.Endpoints;

/// <summary>Rutas para subir y mostrar imágenes.</summary>
public static class ImageEndpoints
{
    private const string LongCache = "public, max-age=31536000, immutable";

    /// <summary>Registra las rutas de imágenes.</summary>
    public static IEndpointRouteBuilder MapImageEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/images/{id:guid}", GetAsync).RequireRateLimiting(RateLimits.PublicReads);
        routes.MapPost("/api/images", UploadAsync).RequireAuthorization(Policies.Owner);
        return routes;
    }

    /// <summary>Muestra una imagen; nunca cambia, así que el navegador la guarda por un año.</summary>
    private static async Task<IResult> GetAsync(Guid id, HttpContext context, GetImageHandler handler, CancellationToken cancellationToken)
    {
        Result<StoredImage> image = await handler.HandleAsync(id, cancellationToken);
        if (image.IsFailure)
        {
            return ApiResults.Problem(image.Error!);
        }

        context.Response.Headers.CacheControl = LongCache;
        context.Response.Headers["Cross-Origin-Resource-Policy"] = "cross-origin";
        return Results.File(image.Value.Data, image.Value.ContentType);
    }

    /// <summary>Recibe la imagen como cuerpo de la petición, sin pasar del tamaño máximo.</summary>
    private static async Task<IResult> UploadAsync(HttpRequest request, UploadImageHandler handler, CancellationToken cancellationToken)
    {
        if (request.ContentLength > StoredImage.MaxBytes)
        {
            return ApiResults.Problem(MediaErrors.TooLarge);
        }

        byte[] chunk = new byte[StoredImage.MaxBytes + 1];
        int total = 0;
        int read;
        while ((read = await request.Body.ReadAsync(chunk.AsMemory(total, chunk.Length - total), cancellationToken)) > 0)
        {
            total += read;
            if (total > StoredImage.MaxBytes)
            {
                return ApiResults.Problem(MediaErrors.TooLarge);
            }
        }

        Result<StoredImage> image = await handler.HandleAsync(chunk[..total], cancellationToken);
        return ApiResults.Created(image, saved => new ImageResponse(saved.Id), saved => $"/api/images/{saved.Id}");
    }
}
