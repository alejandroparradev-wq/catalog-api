using System.Text.Json;
using Reqnroll;

namespace CatalogApi.Specs.Support;

/// <summary>
/// Estado compartido entre los pasos de un mismo escenario.
/// Reqnroll inyecta la misma instancia a todos los [Binding] del escenario.
/// </summary>
public sealed class ApiContext
{
    public HttpResponseMessage? LastResponse { get; set; }

    /// <summary>Raíz del último body JSON (objeto), clonada para que sobreviva al dispose del parseo.</summary>
    public JsonElement? LastJson { get; set; }

    /// <summary>Elementos del último body JSON cuando fue un arreglo.</summary>
    public List<JsonElement>? LastList { get; set; }

    /// <summary>Propiedad "message" del último body, si existía. Es el mensaje de negocio.</summary>
    public string? LastError { get; set; }

    /// <summary>Últimos valores enviados en la última edición, para los pasos "queda actualizada".</summary>
    public string? LastEditName { get; set; }
    public string? LastEditDescription { get; set; }
    public string? LastEditCategory { get; set; }

    /// <summary>Mapea nombre -> Id según se van creando entidades en el escenario.</summary>
    public Dictionary<string, int> CategoryIds { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> ProductIds { get; } = new(StringComparer.OrdinalIgnoreCase);

    public async Task CaptureAsync(HttpResponseMessage response)
    {
        LastResponse = response;
        LastJson = null;
        LastList = null;
        LastError = null;

        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body))
            return;

        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            LastList = doc.RootElement.EnumerateArray().Select(e => e.Clone()).ToList();
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            LastJson = doc.RootElement.Clone();
            if (doc.RootElement.TryGetProperty("message", out var message))
                LastError = message.GetString();
        }
    }

    /// <summary>
    /// Las tablas de Gherkin se escribieron en estilo "clave | valor" SIN fila de
    /// encabezado propia. Reqnroll consume la primera fila como encabezado, así que
    /// reconstruimos los pares así: encabezado = primer par, cada fila = un par más.
    /// Ej.: | Nombre | Bebidas | -> ("Nombre" -> "Bebidas").
    /// </summary>
    public static Dictionary<string, string> ParseKeyValueTable(DataTable table) =>
        BuildPairs(table.Header.ToList(), table.Rows.ToList());

    private static Dictionary<string, string> BuildPairs(List<string> header, List<DataTableRow> rows)
    {
        var pairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (header.Count >= 2)
            pairs[header[0]] = header[1];

        foreach (var row in rows)
        {
            if (row.Count >= 2)
                pairs[row[0]] = row[1];
        }

        return pairs;
    }
}