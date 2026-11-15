using Barberia.Application.Ports;
using Barberia.Domain.Common;
using Barberia.Domain.Media;
using Barberia.Domain.Shops;
using Barberia.Domain.Staff;

namespace Barberia.Application.Features.Media;

/// <summary>Guarda una imagen nueva después de revisar su formato y su tamaño.</summary>
public sealed class UploadImageHandler(IImageRepository images, IUnitOfWork unitOfWork)
{
    /// <summary>Guarda la imagen y devuelve su id.</summary>
    public async Task<Result<StoredImage>> HandleAsync(byte[] data, CancellationToken cancellationToken)
    {
        Result<StoredImage> image = StoredImage.Create(data);
        if (image.IsFailure)
        {
            return image;
        }

        images.Add(image.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return image;
    }
}

/// <summary>Entrega una imagen para mostrarla.</summary>
public sealed class GetImageHandler(IImageRepository images)
{
    /// <summary>Devuelve la imagen o el error de no encontrada.</summary>
    public async Task<Result<StoredImage>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        StoredImage? image = await images.GetAsync(id, cancellationToken);
        if (image is null)
        {
            return MediaErrors.NotFound;
        }

        return image;
    }
}

/// <summary>Cambia el logo y las fotos del local, y borra las imágenes que dejan de usarse.</summary>
public sealed class ShopImagesHandler(IShopRepository shops, IImageRepository images, IUnitOfWork unitOfWork)
{
    /// <summary>Pone o quita el logo.</summary>
    public async Task<Result<Shop>> SetLogoAsync(Guid? imageId, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        if (imageId is not null && !await images.AllExistAsync([imageId.Value], cancellationToken))
        {
            return MediaErrors.NotFound;
        }

        Guid? previous = shop.ChangeLogo(imageId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await RemoveUnusedAsync(previous, imageId, cancellationToken);
        return shop;
    }

    /// <summary>Reemplaza las fotos del local.</summary>
    public async Task<Result<Shop>> SetGalleryAsync(IReadOnlyList<Guid> imageIds, CancellationToken cancellationToken)
    {
        Shop? shop = await shops.GetAsync(cancellationToken);
        if (shop is null)
        {
            return ShopErrors.NotConfigured;
        }

        if (!await images.AllExistAsync(imageIds, cancellationToken))
        {
            return MediaErrors.NotFound;
        }

        Result<IReadOnlyList<Guid>> removed = shop.ChangeGallery(imageIds);
        if (removed.IsFailure)
        {
            return removed.Error!;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await images.RemoveAsync(removed.Value, cancellationToken);
        return shop;
    }

    /// <summary>Borra la imagen anterior si ya no es la actual.</summary>
    private async Task RemoveUnusedAsync(Guid? previous, Guid? current, CancellationToken cancellationToken)
    {
        if (previous is not null && previous != current)
        {
            await images.RemoveAsync([previous.Value], cancellationToken);
        }
    }
}

/// <summary>Cambia la foto de un barbero y borra la anterior.</summary>
public sealed class BarberPhotoHandler(IBarberRepository barbers, IImageRepository images, IUnitOfWork unitOfWork)
{
    /// <summary>Pone o quita la foto del barbero.</summary>
    public async Task<Result<Barber>> HandleAsync(Guid barberId, Guid? imageId, CancellationToken cancellationToken)
    {
        Barber? barber = await barbers.GetAsync(barberId, cancellationToken);
        if (barber is null)
        {
            return StaffErrors.NotFound;
        }

        if (imageId is not null && !await images.AllExistAsync([imageId.Value], cancellationToken))
        {
            return MediaErrors.NotFound;
        }

        Guid? previous = barber.ChangePhoto(imageId);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        if (previous is not null && previous != imageId)
        {
            await images.RemoveAsync([previous.Value], cancellationToken);
        }

        return barber;
    }
}
