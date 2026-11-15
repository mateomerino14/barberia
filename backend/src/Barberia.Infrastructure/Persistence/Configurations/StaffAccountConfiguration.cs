using Barberia.Domain.Access;
using Barberia.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barberia.Infrastructure.Persistence.Configurations;

/// <summary>Tabla de cuentas del panel, con el correo único.</summary>
public sealed class StaffAccountConfiguration : IEntityTypeConfiguration<StaffAccount>
{
    private const int RoleMaxLength = 20;

    /// <summary>Configura la tabla de cuentas.</summary>
    public void Configure(EntityTypeBuilder<StaffAccount> builder)
    {
        builder.ToTable("staff_accounts");
        builder.HasKey(account => account.Id);
        builder.Property(account => account.Email).HasMaxLength(Limits.EmailMaxLength);
        builder.HasIndex(account => account.Email).IsUnique();
        builder.Property(account => account.Role).HasConversion<string>().HasMaxLength(RoleMaxLength);
        builder.HasIndex(account => account.BarberId);
    }
}
