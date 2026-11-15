using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barberia.Infrastructure.Persistence.Configurations;

/// <summary>Tabla de bloqueos de agenda.</summary>
public sealed class BlockConfiguration : IEntityTypeConfiguration<Block>
{
    /// <summary>Configura la tabla de bloqueos.</summary>
    public void Configure(EntityTypeBuilder<Block> builder)
    {
        builder.ToTable("blocks");
        builder.HasKey(block => block.Id);
        builder.Property(block => block.Reason).HasMaxLength(Limits.ReasonMaxLength);
        builder.OwnsOne(block => block.Range, range =>
        {
            range.Property(item => item.Start).HasColumnName("start_at");
            range.Property(item => item.End).HasColumnName("end_at");
        });
        builder.Navigation(block => block.Range).IsRequired();
        builder.HasIndex(block => block.BarberId);
    }
}
