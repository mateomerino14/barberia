using Barberia.Application.Ports;
using Barberia.Domain.Media;
using Microsoft.EntityFrameworkCore;

namespace Barberia.Infrastructure.Persistence.Repositories;

/// <summary>Adaptador de PostgreSQL para las imágenes, guardadas como bytes en su propia tabla.</summary>
public sealed class ImageRepository(BarberiaDbContext context) : IImageRepository
{
    /// <summary>Busca una imagen con su contenido, sin seguirla en memoria.</summary>
    public Task<StoredImage?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.Images.AsNoTracking().FirstOrDefaultAsync(image => image.Id == id, cancellationToken);
    }

    /// <summary>Indica si existen todas las imágenes indicadas.</summary>
    public async Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        List<Guid> unique = [.. ids.Distinct()];
        int found = await context.Images.CountAsync(image => unique.Contains(image.Id), cancellationToken);
        return found == unique.Count;
    }

    /// <summary>Agrega una imagen.</summary>
    public void Add(StoredImage image)
    {
        context.Images.Add(image);
    }

    /// <summary>Borra las imágenes que ya no se usan.</summary>
    public async Task RemoveAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return;
        }

        List<Guid> unique = [.. ids.Distinct()];
        await context.Images.Where(image => unique.Contains(image.Id)).ExecuteDeleteAsync(cancellationToken);
    }
}
