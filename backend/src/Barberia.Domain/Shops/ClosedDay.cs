namespace Barberia.Domain.Shops;

/// <summary>Fecha puntual en la que la barbería no abre, como un feriado.</summary>
public sealed record ClosedDay(DateOnly Date, string Reason);
