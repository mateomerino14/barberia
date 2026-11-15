using Barberia.Domain.Common;

namespace Barberia.Domain.Media;

/// <summary>Errores de negocio de las imágenes.</summary>
public static class MediaErrors
{
    /// <summary>La imagen está vacía o pesa más de 2 MB.</summary>
    public static readonly Error TooLarge = Error.Validation("media.too_large", "La imagen tiene que pesar menos de 2 MB.");

    /// <summary>El archivo no es JPEG, PNG ni WebP.</summary>
    public static readonly Error UnsupportedType = Error.Validation("media.unsupported_type", "Sube una foto en JPG, PNG o WebP.");

    /// <summary>La imagen no existe.</summary>
    public static readonly Error NotFound = Error.NotFound("media.not_found", "No encontramos esa imagen.");

    /// <summary>Hay más fotos del local de las permitidas.</summary>
    public static readonly Error TooManyPhotos = Error.Validation("media.too_many_photos", "Puedes mostrar hasta 6 fotos del local.");
}
