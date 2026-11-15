using Barberia.Domain.Common;
using Barberia.Domain.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barberia.Infrastructure.Persistence.Configurations;

/// <summary>Tabla de turnos, con número correlativo y búsqueda por barbero y hora.</summary>
public sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    private const int StatusMaxLength = 20;
    private const int TokenMaxLength = 64;

    /// <summary>Configura la tabla de turnos.</summary>
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");
        builder.HasKey(booking => booking.Id);
        builder.Property(booking => booking.Number).HasDefaultValueSql("nextval('booking_numbers')").ValueGeneratedOnAdd();
        builder.HasIndex(booking => booking.Number).IsUnique();
        builder.Property(booking => booking.ServiceName).HasMaxLength(Limits.NameMaxLength);
        builder.Property(booking => booking.Status).HasConversion<string>().HasMaxLength(StatusMaxLength);
        builder.Property(booking => booking.ManageToken).HasMaxLength(TokenMaxLength);
        builder.HasIndex(booking => booking.ManageToken).IsUnique();
        builder.OwnsOne(booking => booking.Range, range =>
        {
            range.Property(item => item.Start).HasColumnName("start_at");
            range.Property(item => item.End).HasColumnName("end_at");
            range.HasIndex(item => item.Start);
        });
        builder.Navigation(booking => booking.Range).IsRequired();
        builder.OwnsOne(booking => booking.Customer, customer =>
        {
            customer.Property(item => item.Name).HasColumnName("customer_name").HasMaxLength(Limits.NameMaxLength);
            customer.Property(item => item.Phone).HasColumnName("customer_phone").HasMaxLength(Limits.PhoneMaxLength);
            customer.Property(item => item.Email).HasColumnName("customer_email").HasMaxLength(Limits.EmailMaxLength);
        });
        builder.Navigation(booking => booking.Customer).IsRequired();
        builder.HasIndex(booking => booking.BarberId);
    }
}
