using Barberia.Domain.Common;

namespace Barberia.Domain.Media;

/// <summary>Imagen subida por el dueño: logo, foto del local o de un barbero.</summary>
public sealed class StoredImage : Entity
{
    /// <summary>Tamaño máximo de una imagen ya reducida por la web: 2 MB.</summary>
    public const int MaxBytes = 2 * 1024 * 1024;

    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] RiffSignature = [0x52, 0x49, 0x46, 0x46];
    private static readonly byte[] WebpSignature = [0x57, 0x45, 0x42, 0x50];
    private const int WebpMarkerOffset = 8;

    /// <summary>Constructor para la base de datos.</summary>
    private StoredImage()
    {
    }

    /// <summary>Tipo de la imagen: image/jpeg, image/png o image/webp.</summary>
    public string ContentType { get; private init; } = string.Empty;

    /// <summary>Contenido de la imagen.</summary>
    public byte[] Data { get; private init; } = [];

    /// <summary>Crea la imagen si su contenido es de verdad JPEG, PNG o WebP y no pasa del tamaño máximo.</summary>
    public static Result<StoredImage> Create(byte[] data)
    {
        if (data.Length == 0 || data.Length > MaxBytes)
        {
            return MediaErrors.TooLarge;
        }

        string? contentType = Detect(data);
        if (contentType is null)
        {
            return MediaErrors.UnsupportedType;
        }

        return new StoredImage { ContentType = contentType, Data = data };
    }

    /// <summary>Reconoce el formato por los primeros bytes, sin fiarse de lo que diga el navegador.</summary>
    private static string? Detect(ReadOnlySpan<byte> data)
    {
        if (data.StartsWith(JpegSignature))
        {
            return "image/jpeg";
        }

        if (data.StartsWith(PngSignature))
        {
            return "image/png";
        }

        if (data.StartsWith(RiffSignature) && data.Length > WebpMarkerOffset + WebpSignature.Length
            && data[WebpMarkerOffset..].StartsWith(WebpSignature))
        {
            return "image/webp";
        }

        return null;
    }
}
