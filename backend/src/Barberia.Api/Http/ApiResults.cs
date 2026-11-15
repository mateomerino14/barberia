using Barberia.Domain.Common;

namespace Barberia.Api.Http;

/// <summary>Traduce los resultados de los casos de uso a respuestas HTTP.</summary>
public static class ApiResults
{
    private const string CodeKey = "code";

    /// <summary>Responde 200 con el valor convertido, o el problema si el caso de uso falló.</summary>
    public static IResult Ok<T, TResponse>(Result<T> result, Func<T, TResponse> map)
    {
        if (result.IsFailure)
        {
            return Problem(result.Error!);
        }

        return Results.Ok(map(result.Value));
    }

    /// <summary>Responde 201 con el valor convertido, o el problema si el caso de uso falló.</summary>
    public static IResult Created<T, TResponse>(Result<T> result, Func<T, TResponse> map, Func<T, string> location)
    {
        if (result.IsFailure)
        {
            return Problem(result.Error!);
        }

        return Results.Created(location(result.Value), map(result.Value));
    }

    /// <summary>Arma un ProblemDetails con el mensaje en español y el código estable del error.</summary>
    public static IResult Problem(Error error)
    {
        Dictionary<string, object?> extensions = new() { [CodeKey] = error.Code };
        return Results.Problem(title: error.Message, statusCode: StatusFor(error.Kind), extensions: extensions);
    }

    /// <summary>Elige el código HTTP según el tipo de error.</summary>
    private static int StatusFor(ErrorKind kind)
    {
        switch (kind)
        {
            case ErrorKind.NotFound:
                return StatusCodes.Status404NotFound;
            case ErrorKind.Conflict:
                return StatusCodes.Status409Conflict;
            case ErrorKind.Unauthorized:
                return StatusCodes.Status401Unauthorized;
            default:
                return StatusCodes.Status400BadRequest;
        }
    }
}
