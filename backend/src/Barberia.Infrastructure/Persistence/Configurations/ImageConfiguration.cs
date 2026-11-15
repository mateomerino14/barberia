using Barberia.Domain.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barberia.Infrastructure.Persistence.Configurations;

/// <summary>Tabla de las imágenes subidas.</summary>
public sealed class ImageConfiguration : IEntityTypeConfiguration<StoredImage>
{
    private const int ContentTypeMaxLength = 20;

    /// <summary>Configura la tabla de imágenes.</summary>
    public void Configure(EntityTypeBuilder<StoredImage> builder)
    {
        builder.ToTable("images");
        builder.HasKey(image => image.Id);
        builder.Property(image => image.ContentType).HasMaxLength(ContentTypeMaxLength);
        builder.Property(image => image.Data);
    }
}
