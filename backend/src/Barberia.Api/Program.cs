using System.Text.Json;
using System.Text.Json.Serialization;
using Barberia.Api.Endpoints;
using Barberia.Api.Http;
using Barberia.Application;
using Barberia.Application.Features.Shops;
using Barberia.Domain.Common;
using Barberia.Infrastructure;

const string CorsPolicy = "web";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddPanelPolicies();
builder.Services.AddPublicRateLimits(builder.Configuration);
builder.Services.AddProxySupport(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});
string[] origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

WebApplication app = builder.Build();
app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseSecurityHeaders();
app.UseExceptionHandler();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHealthEndpoints();
app.MapAccessEndpoints();
app.MapShopEndpoints();
app.MapCatalogEndpoints();
app.MapStaffEndpoints();
app.MapBookingEndpoints();
app.MapAgendaEndpoints();

await app.Services.MigrateDatabaseAsync(CancellationToken.None);
using (IServiceScope scope = app.Services.CreateScope())
{
    InitializeShopHandler initialize = scope.ServiceProvider.GetRequiredService<InitializeShopHandler>();
    IConfigurationSection setup = app.Configuration.GetSection("Setup");
    Result initialized = await initialize.HandleAsync(
        new InitializeShopCommand(setup["ShopName"], setup["OwnerEmail"], setup["OwnerPassword"]),
        CancellationToken.None);
    if (initialized.IsFailure)
    {
        throw new InvalidOperationException($"No se pudo preparar la barbería: {initialized.Error!.Message}");
    }

    if (setup.GetValue<bool>("DemoData"))
    {
        SeedDemoHandler demo = scope.ServiceProvider.GetRequiredService<SeedDemoHandler>();
        await demo.HandleAsync(CancellationToken.None);
    }
}

await app.RunAsync();
