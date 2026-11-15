using Barberia.Domain.Catalog;
using Barberia.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barberia.Infrastructure.Persistence.Configurations;

/// <summary>Tabla de servicios, con el precio guardado en centavos.</summary>
public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    /// <summary>Configura la tabla de servicios.</summary>
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");
        builder.HasKey(service => service.Id);
        builder.Property(service => service.Name).HasMaxLength(Limits.NameMaxLength);
        builder.Property(service => service.Price)
            .HasColumnName("price_cents")
            .HasConversion(money => money.Cents, cents => Money.FromCents(cents).Value);
    }
}
