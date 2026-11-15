using Barberia.Application.Ports;
using Barberia.Infrastructure.Persistence;
using Barberia.Infrastructure.Persistence.Repositories;
using Barberia.Infrastructure.Security;
using Barberia.Infrastructure.Time;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Barberia.Infrastructure;

/// <summary>Registra los adaptadores de salida en el contenedor de dependencias.</summary>
public static class DependencyInjection
{
    private const string ConnectionName = "Barberia";
    private const int MinKeyLength = 32;

    /// <summary>Agrega la base de datos, la seguridad y el reloj.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connection = configuration.GetConnectionString(ConnectionName)
            ?? throw new InvalidOperationException("Falta la cadena de conexión ConnectionStrings:Barberia.");
        services.AddDbContext<BarberiaDbContext>(options => options.UseNpgsql(connection).UseSnakeCaseNamingConvention());
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<BarberiaDbContext>());
        services.AddScoped<IShopRepository, ShopRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IBarberRepository, BarberRepository>();
        services.AddScoped<IStaffAccountRepository, StaffAccountRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBlockRepository, BlockRepository>();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenIssuer, TokenIssuer>();
        services.AddPanelAuthentication(configuration);
        return services;
    }

    /// <summary>Configura la lectura del pase de sesión en cada petición.</summary>
    private static void AddPanelAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection section = configuration.GetSection(JwtOptions.Section);
        JwtOptions settings = section.Get<JwtOptions>() ?? new JwtOptions();
        if (settings.Key.Length < MinKeyLength)
        {
            throw new InvalidOperationException("Jwt:Key tiene que tener al menos 32 caracteres.");
        }

        services.Configure<JwtOptions>(section);
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = settings.Issuer,
                ValidAudience = settings.Audience,
                IssuerSigningKey = TokenIssuer.SigningKey(settings),
                RoleClaimType = StaffClaims.Role,
                NameClaimType = StaffClaims.Email,
            };
        });
    }

    /// <summary>Aplica las migraciones pendientes de la base de datos.</summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        BarberiaDbContext context = scope.ServiceProvider.GetRequiredService<BarberiaDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
