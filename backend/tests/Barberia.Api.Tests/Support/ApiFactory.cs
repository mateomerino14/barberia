using System.Net.Http.Headers;
using System.Net.Http.Json;
using Barberia.Api.Contracts;
using Barberia.Application.Ports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Barberia.Api.Tests.Support;

/// <summary>Levanta la API completa contra un PostgreSQL real en Docker.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>Nombre de la barbería con que arranca la instalación de prueba.</summary>
    public const string ShopName = "Barbería de Prueba";

    /// <summary>Correo del dueño de prueba.</summary>
    public const string OwnerEmail = "dueno@prueba.bo";

    /// <summary>Contraseña del dueño de prueba.</summary>
    public const string OwnerPassword = "contrasena-segura";

    private const string PostgresImage = "postgres:17";
    private const string TestingEnvironment = "Testing";
    private const string JwtKey = "clave-de-prueba-con-mas-de-32-caracteres";
    private const string BearerScheme = "Bearer";
    private const string LoginPath = "/api/auth/login";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder(PostgresImage).Build();

    /// <summary>Arranca la base de datos antes de la primera prueba.</summary>
    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();
    }

    /// <summary>Apaga la API y la base de datos.</summary>
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    /// <summary>Crea un cliente con la sesión de la cuenta indicada.</summary>
    public async Task<HttpClient> CreateClientAsAsync(string email, string password)
    {
        HttpClient client = CreateClient();
        HttpResponseMessage response = await client.PostAsJsonAsync(LoginPath, new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonDefaults.Options);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(BearerScheme, login!.Token);
        return client;
    }

    /// <summary>Crea un cliente con la sesión del dueño.</summary>
    public Task<HttpClient> CreateOwnerClientAsync()
    {
        return CreateClientAsAsync(OwnerEmail, OwnerPassword);
    }

    /// <summary>Apunta la API a la base de prueba y le da su configuración.</summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(TestingEnvironment);
        builder.UseSetting("ConnectionStrings:Barberia", _database.GetConnectionString());
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Setup:ShopName", ShopName);
        builder.UseSetting("Setup:OwnerEmail", OwnerEmail);
        builder.UseSetting("Setup:OwnerPassword", OwnerPassword);
        builder.ConfigureTestServices(services => services.AddSingleton<IClock, FixedClock>());
    }
}

/// <summary>Agrupa las pruebas de la API para que compartan una sola base de datos.</summary>
[CollectionDefinition(Name)]
public sealed class ApiTestGroup : ICollectionFixture<ApiFactory>
{
    /// <summary>Nombre de la colección.</summary>
    public const string Name = "api";
}
