using System.Globalization;
using Barberia.Domain.Common;

namespace Barberia.Domain.Catalog;

/// <summary>Monto en bolivianos guardado en centavos para no perder decimales.</summary>
public readonly record struct Money
{
    private const int CentsPerBoliviano = 100;
    private const int MaxCents = 1_000_000;

    /// <summary>Crea un monto ya validado.</summary>
    private Money(int cents)
    {
        Cents = cents;
    }

    /// <summary>Monto en centavos.</summary>
    public int Cents { get; }

    /// <summary>Crea un monto entre 0 y Bs 10.000.</summary>
    public static Result<Money> FromCents(int cents)
    {
        if (cents < 0 || cents > MaxCents)
        {
            return Error.Validation("money.invalid", "El precio va de Bs 0 a Bs 10.000.");
        }

        return new Money(cents);
    }

    /// <summary>Muestra el monto como lo lee el cliente, por ejemplo "Bs 50" o "Bs 52,50".</summary>
    public override string ToString()
    {
        int whole = Cents / CentsPerBoliviano;
        int rest = Cents % CentsPerBoliviano;
        if (rest == 0)
        {
            return string.Create(CultureInfo.InvariantCulture, $"Bs {whole}");
        }

        return string.Create(CultureInfo.InvariantCulture, $"Bs {whole},{rest:00}");
    }
}
