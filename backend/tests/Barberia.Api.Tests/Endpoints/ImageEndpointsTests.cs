using System.Net;
using System.Net.Http.Headers;
using Barberia.Api.Contracts;
using Barberia.Api.Tests.Support;

namespace Barberia.Api.Tests.Endpoints;

/// <summary>Pruebas de subir y mostrar imágenes: logo, fotos del local y del barbero.</summary>
[Collection(ApiTestGroup.Name)]
public sealed class ImageEndpointsTests(ApiFactory factory)
{
    private const string ImagesPath = "/api/images";
    private const int TooManyPhotos = 7;
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02];

    /// <summary>El dueño sube una imagen, la pone de logo y cualquiera la ve con caché larga.</summary>
    [Fact]
    public async Task Upload_SetLogo_ServedWithLongCache()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();

        Guid id = await UploadAsync(owner, Png);
        ShopResponse shop = await (await owner.PutJsonAsync("/api/shop/logo", new SetLogoRequest(id))).ReadAsync<ShopResponse>();
        HttpResponseMessage image = await factory.CreateClient().GetAsync($"{ImagesPath}/{id}", TestContext.Current.CancellationToken);

        Assert.Equal(id, shop.LogoImageId);
        Assert.Equal(HttpStatusCode.OK, image.StatusCode);
        Assert.Equal("image/png", image.Content.Headers.ContentType?.MediaType);
        Assert.Contains("immutable", image.Headers.CacheControl?.ToString(), StringComparison.Ordinal);
    }

    /// <summary>Al cambiar el logo, la imagen anterior se borra.</summary>
    [Fact]
    public async Task ReplaceLogo_RemovesPrevious()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        Guid first = await UploadAsync(owner, Png);
        Guid second = await UploadAsync(owner, Png);

        await owner.PutJsonAsync("/api/shop/logo", new SetLogoRequest(first));
        await owner.PutJsonAsync("/api/shop/logo", new SetLogoRequest(second));
        HttpResponseMessage old = await factory.CreateClient().GetAsync($"{ImagesPath}/{first}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, old.StatusCode);
    }

    /// <summary>Un archivo que no es imagen no se guarda.</summary>
    [Fact]
    public async Task Upload_NotAnImage_IsRejected()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        ByteArrayContent content = new("<script>alert(1)</script>"u8.ToArray());
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        HttpResponseMessage response = await owner.PostAsync(ImagesPath, content, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("media.unsupported_type", await response.ReadErrorCodeAsync());
    }

    /// <summary>Sin sesión de dueño no se puede subir nada.</summary>
    [Fact]
    public async Task Upload_WithoutOwner_IsRejected()
    {
        HttpResponseMessage response = await factory.CreateClient().PostAsync(ImagesPath, new ByteArrayContent(Png), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    /// <summary>Se muestran hasta seis fotos del local.</summary>
    [Fact]
    public async Task Gallery_MoreThanSix_IsRejected()
    {
        HttpClient owner = await factory.CreateOwnerClientAsync();
        List<Guid> ids = [];
        for (int index = 0; index < TooManyPhotos; index++)
        {
            ids.Add(await UploadAsync(owner, Png));
        }

        HttpResponseMessage tooMany = await owner.PutJsonAsync("/api/shop/gallery", new SetGalleryRequest(ids));
        HttpResponseMessage six = await owner.PutJsonAsync("/api/shop/gallery", new SetGalleryRequest(ids[1..]));

        Assert.Equal(HttpStatusCode.BadRequest, tooMany.StatusCode);
        Assert.Equal(HttpStatusCode.OK, six.StatusCode);
    }

    /// <summary>La foto del barbero aparece en la lista pública.</summary>
    [Fact]
    public async Task BarberPhoto_ShowsInPublicList()
    {
        Scenario scenario = await Scenario.CreateAsync(factory);
        HttpClient owner = await factory.CreateOwnerClientAsync();
        Guid id = await UploadAsync(owner, Png);

        await owner.PutJsonAsync($"/api/barbers/{scenario.BarberId}/photo", new SetPhotoRequest(id));
        IReadOnlyList<PublicBarberResponse> barbers = await (await factory.CreateClient().GetAsync("/api/barbers", TestContext.Current.CancellationToken))
            .ReadAsync<IReadOnlyList<PublicBarberResponse>>();

        Assert.Contains(barbers, barber => barber.Id == scenario.BarberId && barber.PhotoImageId == id);
    }

    /// <summary>Sube una imagen y devuelve su id.</summary>
    private static async Task<Guid> UploadAsync(HttpClient client, byte[] data)
    {
        ByteArrayContent content = new(data);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        HttpResponseMessage response = await client.PostAsync(ImagesPath, content, TestContext.Current.CancellationToken);
        return (await response.ReadAsync<ImageResponse>()).Id;
    }
}
