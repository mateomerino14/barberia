using System.Net.Http.Json;
using System.Text.Json;

namespace Barberia.Api.Tests.Support;

/// <summary>Atajos para enviar y leer JSON con las opciones de la API.</summary>
public static class HttpExtensions
{
    private const string CodeKey = "code";

    /// <summary>Envía un POST con cuerpo JSON.</summary>
    public static Task<HttpResponseMessage> PostJsonAsync<T>(this HttpClient client, string path, T body)
    {
        return client.PostAsJsonAsync(path, body, JsonDefaults.Options, TestContext.Current.CancellationToken);
    }

    /// <summary>Envía un PUT con cuerpo JSON.</summary>
    public static Task<HttpResponseMessage> PutJsonAsync<T>(this HttpClient client, string path, T body)
    {
        return client.PutAsJsonAsync(path, body, JsonDefaults.Options, TestContext.Current.CancellationToken);
    }

    /// <summary>Lee el cuerpo JSON de la respuesta.</summary>
    public static async Task<T> ReadAsync<T>(this HttpResponseMessage response)
    {
        T? value = await response.Content.ReadFromJsonAsync<T>(JsonDefaults.Options, TestContext.Current.CancellationToken);
        return value!;
    }

    /// <summary>Lee el código estable del error en un ProblemDetails.</summary>
    public static async Task<string?> ReadErrorCodeAsync(this HttpResponseMessage response)
    {
        JsonElement problem = await response.ReadAsync<JsonElement>();
        if (problem.TryGetProperty(CodeKey, out JsonElement code))
        {
            return code.GetString();
        }

        return null;
    }
}
