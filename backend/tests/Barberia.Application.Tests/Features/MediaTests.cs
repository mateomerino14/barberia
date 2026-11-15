using Barberia.Application.Features.Media;
using Barberia.Application.Tests.Fakes;
using Barberia.Domain.Common;
using Barberia.Domain.Media;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Tests.Features;

/// <summary>Pruebas de imágenes con puertos falsos: subir, ver, logo, fotos del local y del barbero.</summary>
public sealed class MediaTests
{
    private const int TooManyPhotos = 7;
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01];
    private static readonly byte[] NotAnImage = [0x3C, 0x68, 0x74, 0x6D, 0x6C];

    /// <summary>Una imagen válida se guarda y se puede ver; un archivo que no es imagen no se guarda.</summary>
    [Fact]
    public async Task Upload_ValidAndInvalid()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        UploadImageHandler upload = new(world.Store, world.Store);

        Result<StoredImage> saved = await upload.HandleAsync(Png, TestContext.Current.CancellationToken);
        Result<StoredImage> rejected = await upload.HandleAsync(NotAnImage, TestContext.Current.CancellationToken);
        Result<StoredImage> shown = await new GetImageHandler(world.Store).HandleAsync(saved.Value.Id, TestContext.Current.CancellationToken);
        Result<StoredImage> missing = await new GetImageHandler(world.Store).HandleAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.Equal(MediaErrors.UnsupportedType, rejected.Error);
        Assert.Equal("image/png", shown.Value.ContentType);
        Assert.Equal(MediaErrors.NotFound, missing.Error);
    }

    /// <summary>Al cambiar el logo, el anterior se borra.</summary>
    [Fact]
    public async Task Logo_ReplaceRemovesPrevious()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        Guid first = await UploadAsync(world);
        Guid second = await UploadAsync(world);
        ShopImagesHandler handler = new(world.Store, world.Store, world.Store);

        await handler.SetLogoAsync(first, TestContext.Current.CancellationToken);
        Result<Shop> shop = await handler.SetLogoAsync(second, TestContext.Current.CancellationToken);

        Assert.Equal(second, shop.Value.LogoImageId);
        Assert.DoesNotContain(world.Store.Images, image => image.Id == first);
    }

    /// <summary>Se aceptan hasta seis fotos del local, y las que salen se borran.</summary>
    [Fact]
    public async Task Gallery_LimitAndRemoval()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        List<Guid> ids = [];
        for (int index = 0; index < TooManyPhotos; index++)
        {
            ids.Add(await UploadAsync(world));
        }

        ShopImagesHandler handler = new(world.Store, world.Store, world.Store);

        Result<Shop> tooMany = await handler.SetGalleryAsync(ids, TestContext.Current.CancellationToken);
        await handler.SetGalleryAsync(ids[1..], TestContext.Current.CancellationToken);
        Result<Shop> fewer = await handler.SetGalleryAsync(ids[2..], TestContext.Current.CancellationToken);

        Assert.Equal(MediaErrors.TooManyPhotos, tooMany.Error);
        Assert.Equal(ids.Count - 2, fewer.Value.GalleryImageIds.Count);
        Assert.DoesNotContain(world.Store.Images, image => image.Id == ids[1]);
    }

    /// <summary>La foto de un barbero se pone y se quita; con una imagen que no existe da error.</summary>
    [Fact]
    public async Task BarberPhoto_SetRemoveAndUnknown()
    {
        DemoWorld world = await DemoWorld.CreateAsync();
        Guid photo = await UploadAsync(world);
        BarberPhotoHandler handler = new(world.Store, world.Store, world.Store);

        Guid? afterSet = (await handler.HandleAsync(world.Lucho.Id, photo, TestContext.Current.CancellationToken)).Value.PhotoImageId;
        Result<Barber> unknown = await handler.HandleAsync(world.Lucho.Id, Guid.NewGuid(), TestContext.Current.CancellationToken);
        Result<Barber> removed = await handler.HandleAsync(world.Lucho.Id, null, TestContext.Current.CancellationToken);

        Assert.Equal(photo, afterSet);
        Assert.Equal(MediaErrors.NotFound, unknown.Error);
        Assert.Null(removed.Value.PhotoImageId);
        Assert.Empty(world.Store.Images);
    }

    /// <summary>Sube una imagen válida y devuelve su id.</summary>
    private static async Task<Guid> UploadAsync(DemoWorld world)
    {
        return (await new UploadImageHandler(world.Store, world.Store).HandleAsync(Png, TestContext.Current.CancellationToken)).Value.Id;
    }
}
