using Barberia.Api.Endpoints;
using Barberia.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure();

WebApplication app = builder.Build();
app.MapHealthEndpoints();
app.Run();
