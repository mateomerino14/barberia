using Barberia.Domain.Common;
using Barberia.Domain.Media;

namespace Barberia.Domain.Tests.Media;

/// <summary>Pruebas de la validación de imágenes.</summary>
public sealed class StoredImageTests
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0];
    private static readonly byte[] Webp = [0x52, 0x49, 0x46, 0x46, 0x00, 0x00, 0x00, 0x00, 0x57, 0x45, 0x42, 0x50, 0x00];
    private static readonly byte[] Html = "<html><script>alert(1)</script></html>"u8.ToArray();

    /// <summary>Reconoce el formato por su contenido.</summary>
    [Fact]
    public void Create_KnownFormats_DetectsTypeFromBytes()
    {
        Assert.Equal("image/png", StoredImage.Create(Png).Value.ContentType);
        Assert.Equal("image/jpeg", StoredImage.Create(Jpeg).Value.ContentType);
        Assert.Equal("image/webp", StoredImage.Create(Webp).Value.ContentType);
    }

    /// <summary>Un archivo que no es imagen se rechaza aunque diga serlo.</summary>
    [Fact]
    public void Create_NotAnImage_IsRejected()
    {
        Result<StoredImage> result = StoredImage.Create(Html);

        Assert.Equal(MediaErrors.UnsupportedType, result.Error);
    }

    /// <summary>Vacía o de más de 2 MB se rechaza.</summary>
    [Fact]
    public void Create_EmptyOrTooLarge_IsRejected()
    {
        byte[] large = new byte[StoredImage.MaxBytes + 1];
        Png.CopyTo(large, 0);

        Assert.Equal(MediaErrors.TooLarge, StoredImage.Create([]).Error);
        Assert.Equal(MediaErrors.TooLarge, StoredImage.Create(large).Error);
    }
}
