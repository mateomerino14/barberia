namespace Barberia.Domain.Common;

/// <summary>Resultado de una operación que puede fallar por una regla de negocio.</summary>
public class Result
{
    /// <summary>Crea un resultado con o sin error.</summary>
    protected Result(Error? error)
    {
        Error = error;
    }

    /// <summary>Error de la operación, o nulo si salió bien.</summary>
    public Error? Error { get; }

    /// <summary>Indica si la operación salió bien.</summary>
    public bool IsSuccess => Error is null;

    /// <summary>Indica si la operación falló.</summary>
    public bool IsFailure => Error is not null;

    /// <summary>Crea un resultado exitoso.</summary>
    public static Result Success()
    {
        return new Result(null);
    }

    /// <summary>Crea un resultado fallido.</summary>
    public static Result Failure(Error error)
    {
        return new Result(error);
    }

    /// <summary>Convierte un error en un resultado fallido.</summary>
    public static implicit operator Result(Error error)
    {
        return Failure(error);
    }
}

/// <summary>Resultado de una operación que devuelve un valor si sale bien.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>Crea un resultado con valor o con error.</summary>
    private Result(T? value, Error? error) : base(error)
    {
        _value = value;
    }

    /// <summary>Valor de la operación; solo se puede leer si salió bien.</summary>
    public T Value
    {
        get
        {
            if (IsFailure || _value is null)
            {
                throw new InvalidOperationException("No se puede leer el valor de un resultado fallido.");
            }

            return _value;
        }
    }

    /// <summary>Crea un resultado exitoso con su valor.</summary>
    public static Result<T> Success(T value)
    {
        return new Result<T>(value, null);
    }

    /// <summary>Crea un resultado fallido del tipo pedido.</summary>
    public static new Result<T> Failure(Error error)
    {
        return new Result<T>(default, error);
    }

    /// <summary>Convierte un valor en un resultado exitoso.</summary>
    public static implicit operator Result<T>(T value)
    {
        return Success(value);
    }

    /// <summary>Convierte un error en un resultado fallido.</summary>
    public static implicit operator Result<T>(Error error)
    {
        return Failure(error);
    }
}
