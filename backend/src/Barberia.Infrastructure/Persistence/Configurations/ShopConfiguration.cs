using Barberia.Domain.Common;
using Barberia.Domain.Shops;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barberia.Infrastructure.Persistence.Configurations;

/// <summary>Tablas de la barbería, su horario y sus días cerrados.</summary>
public sealed class ShopConfiguration : IEntityTypeConfiguration<Shop>
{
    private const int PaletteMaxLength = 20;

    /// <summary>Configura la tabla de la barbería y sus colecciones.</summary>
    public void Configure(EntityTypeBuilder<Shop> builder)
    {
        builder.ToTable("shops");
        builder.HasKey(shop => shop.Id);
        builder.Property(shop => shop.Name).HasMaxLength(Limits.NameMaxLength);
        builder.Property(shop => shop.Address).HasMaxLength(Limits.AddressMaxLength);
        builder.Property(shop => shop.Phone).HasMaxLength(Limits.PhoneMaxLength);
        builder.Property(shop => shop.WelcomeMessage).HasMaxLength(Limits.WelcomeMaxLength);
        builder.Property(shop => shop.Palette).HasConversion<string>().HasMaxLength(PaletteMaxLength).HasDefaultValue(ShopPalette.Classic);
        builder.PrimitiveCollection(shop => shop.GalleryImageIds).HasField("_galleryImageIds");
        builder.OwnsOne(shop => shop.Rules, rules =>
        {
            rules.Property(item => item.MinLeadMinutes).HasColumnName("min_lead_minutes");
            rules.Property(item => item.MaxDaysAhead).HasColumnName("max_days_ahead");
            rules.Property(item => item.CancelLimitMinutes).HasColumnName("cancel_limit_minutes");
            rules.Property(item => item.SlotStepMinutes).HasColumnName("slot_step_minutes").HasDefaultValue(BookingRules.DefaultSlotStepMinutes);
        });
        builder.Navigation(shop => shop.Rules).IsRequired();
        builder.OwnsMany(shop => shop.OpeningHours, hours =>
        {
            hours.ToTable("shop_opening_hours");
            hours.WithOwner().HasForeignKey("shop_id");
            hours.HasKey("shop_id", nameof(DailyHours.Day));
        });
        builder.Navigation(shop => shop.OpeningHours).HasField("_openingHours");
        builder.OwnsMany(shop => shop.ClosedDays, days =>
        {
            days.ToTable("shop_closed_days");
            days.WithOwner().HasForeignKey("shop_id");
            days.HasKey("shop_id", nameof(ClosedDay.Date));
            days.Property(day => day.Reason).HasMaxLength(Limits.ReasonMaxLength);
        });
        builder.Navigation(shop => shop.ClosedDays).HasField("_closedDays");
    }
}
