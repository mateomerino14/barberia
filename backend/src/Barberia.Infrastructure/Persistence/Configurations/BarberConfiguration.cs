using Barberia.Domain.Common;
using Barberia.Domain.Staff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barberia.Infrastructure.Persistence.Configurations;

/// <summary>Tablas de barberos, sus horarios y sus ausencias.</summary>
public sealed class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    /// <summary>Configura la tabla de barberos y sus colecciones.</summary>
    public void Configure(EntityTypeBuilder<Barber> builder)
    {
        builder.ToTable("barbers");
        builder.HasKey(barber => barber.Id);
        builder.Property(barber => barber.Name).HasMaxLength(Limits.NameMaxLength);
        builder.PrimitiveCollection(barber => barber.ServiceIds).HasField("_serviceIds");
        builder.OwnsMany(barber => barber.Schedule, hours =>
        {
            hours.ToTable("barber_schedules");
            hours.WithOwner().HasForeignKey("barber_id");
            hours.HasKey("barber_id", nameof(DailyHours.Day)).HasName("pk_barber_schedules");
        });
        builder.Navigation(barber => barber.Schedule).HasField("_schedule");
        builder.OwnsMany(barber => barber.TimeOff, timeOff =>
        {
            timeOff.ToTable("barber_time_off");
            timeOff.WithOwner().HasForeignKey("barber_id");
            timeOff.HasKey(item => item.Id);
            timeOff.Property(item => item.Id).ValueGeneratedNever();
            timeOff.Property(item => item.Reason).HasMaxLength(Limits.ReasonMaxLength);
        });
        builder.Navigation(barber => barber.TimeOff).HasField("_timeOff");
    }
}
