using System.Text.Json;
using System.Text.Json.Serialization;

namespace Barberia.Api.Tests.Support;

/// <summary>Opciones de JSON iguales a las de la API, con los enums como texto.</summary>
public static class JsonDefaults
{
    /// <summary>Opciones compartidas por las pruebas.</summary>
    public static readonly JsonSerializerOptions Options = CreateOptions();

    /// <summary>Arma las opciones con nombres en camelCase y enums como texto.</summary>
    private static JsonSerializerOptions CreateOptions()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
