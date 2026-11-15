using Barberia.Api.Contracts;
using Barberia.Api.Http;
using Barberia.Application.Features.Access;
using Barberia.Application.Features.Staff;
using Barberia.Domain.Access;
using Barberia.Domain.Staff;

namespace Barberia.Api.Endpoints;

/// <summary>Rutas del equipo de barberos.</summary>
public static class StaffEndpoints
{
    /// <summary>Registra las rutas de barberos.</summary>
    public static IEndpointRouteBuilder MapStaffEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/barbers", ListActiveAsync);
        routes.MapGet("/api/barbers/all", ListAllAsync).RequireAuthorization(Policies.Staff);
        RouteGroupBuilder owner = routes.MapGroup("/api/barbers").RequireAuthorization(Policies.Owner);
        owner.MapGet("/access", ListAccessAsync);
        owner.MapPut("/{id:guid}/access", SetAccessAsync);
        owner.MapPost("/", CreateAsync);
        owner.MapPut("/{id:guid}", UpdateAsync);
        owner.MapPut("/{id:guid}/schedule", SetScheduleAsync);
        owner.MapPost("/{id:guid}/time-off", AddTimeOffAsync);
        owner.MapDelete("/{id:guid}/time-off/{timeOffId:guid}", RemoveTimeOffAsync);
        return routes;
    }

    /// <summary>Lista los barberos que el cliente puede elegir.</summary>
    private static async Task<IResult> ListActiveAsync(ListBarbersHandler handler, CancellationToken cancellationToken)
    {
        IReadOnlyList<Barber> barbers = await handler.HandleAsync(true, cancellationToken);
        return Results.Ok(barbers.Select(PublicBarberResponse.From));
    }

    /// <summary>Lista todo el equipo con sus horarios y ausencias.</summary>
    private static async Task<IResult> ListAllAsync(ListBarbersHandler handler, CancellationToken cancellationToken)
    {
        IReadOnlyList<Barber> barbers = await handler.HandleAsync(false, cancellationToken);
        return Results.Ok(barbers.Select(BarberResponse.From));
    }

    /// <summary>Lista qué barberos pueden entrar al panel y con qué correo.</summary>
    private static async Task<IResult> ListAccessAsync(ListBarberAccessHandler handler, CancellationToken cancellationToken)
    {
        IReadOnlyList<StaffAccount> accounts = await handler.HandleAsync(cancellationToken);
        return Results.Ok(accounts.Select(account => new BarberAccessResponse(account.BarberId!.Value, account.Email)));
    }

    /// <summary>Da acceso al panel a un barbero o le pone una contraseña nueva.</summary>
    private static async Task<IResult> SetAccessAsync(Guid id, BarberAccessRequest request, BarberAccessHandler handler, CancellationToken cancellationToken)
    {
        BarberAccessCommand command = new(id, request.Email, request.Password);
        return ApiResults.Ok(
            await handler.HandleAsync(command, cancellationToken),
            account => new BarberAccessResponse(id, account.Email));
    }

    /// <summary>Crea un barbero y, si se pide, su cuenta del panel.</summary>
    private static async Task<IResult> CreateAsync(SaveBarberRequest request, CreateBarberHandler handler, CancellationToken cancellationToken)
    {
        BarberAccountInput? account = null;
        if (request.Account is not null)
        {
            account = new BarberAccountInput(request.Account.Email, request.Account.Password);
        }

        return ApiResults.Created(
            await handler.HandleAsync(ToCommand(request), account, cancellationToken),
            BarberResponse.From,
            barber => $"/api/barbers/{barber.Id}");
    }

    /// <summary>Edita el nombre, el estado y los servicios de un barbero.</summary>
    private static async Task<IResult> UpdateAsync(Guid id, SaveBarberRequest request, UpdateBarberHandler handler, CancellationToken cancellationToken)
    {
        return ApiResults.Ok(await handler.HandleAsync(id, ToCommand(request), cancellationToken), BarberResponse.From);
    }

    /// <summary>Reemplaza el horario semanal de un barbero.</summary>
    private static async Task<IResult> SetScheduleAsync(Guid id, SetHoursRequest request, SetBarberScheduleHandler handler, CancellationToken cancellationToken)
    {
        SetBarberScheduleCommand command = new([.. request.Hours.Select(hours => hours.ToInput())]);
        return ApiResults.Ok(await handler.HandleAsync(id, command, cancellationToken), BarberResponse.From);
    }

    /// <summary>Registra una ausencia.</summary>
    private static async Task<IResult> AddTimeOffAsync(Guid id, AddTimeOffRequest request, AddTimeOffHandler handler, CancellationToken cancellationToken)
    {
        AddTimeOffCommand command = new(request.From, request.To, request.Reason);
        return ApiResults.Ok(await handler.HandleAsync(id, command, cancellationToken), BarberResponse.From);
    }

    /// <summary>Quita una ausencia.</summary>
    private static async Task<IResult> RemoveTimeOffAsync(Guid id, Guid timeOffId, RemoveTimeOffHandler handler, CancellationToken cancellationToken)
    {
        return ApiResults.Ok(await handler.HandleAsync(id, timeOffId, cancellationToken), BarberResponse.From);
    }

    /// <summary>Convierte la petición en el comando del caso de uso.</summary>
    private static SaveBarberCommand ToCommand(SaveBarberRequest request)
    {
        return new SaveBarberCommand(request.Name, request.IsActive, request.ServiceIds ?? []);
    }
}
