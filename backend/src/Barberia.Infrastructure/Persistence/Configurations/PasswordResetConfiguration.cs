using Barberia.Domain.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Barberia.Infrastructure.Persistence.Configurations;

/// <summary>Tabla de enlaces para cambiar la contraseña, buscados por su huella.</summary>
public sealed class PasswordResetConfiguration : IEntityTypeConfiguration<PasswordReset>
{
    private const int FingerprintLength = 64;

    /// <summary>Configura la tabla de enlaces.</summary>
    public void Configure(EntityTypeBuilder<PasswordReset> builder)
    {
        builder.ToTable("password_resets");
        builder.HasKey(reset => reset.Id);
        builder.Property(reset => reset.TokenHash).HasMaxLength(FingerprintLength);
        builder.HasIndex(reset => reset.TokenHash).IsUnique();
        builder.HasIndex(reset => reset.AccountId);
    }
}
