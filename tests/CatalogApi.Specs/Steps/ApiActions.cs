using System.Net.Http.Json;
using System.Text.Json;
using CatalogApi.Specs.Support;

namespace CatalogApi.Specs.Steps;

/// <summary>
/// Acciones HTTP compartidas por las features. Centraliza cómo se crean entidades
/// (y cómo se recuerdan sus Ids), para que "existe una categoría llamada X" signifique
/// lo mismo dentro de Category.feature y dentro de Product.feature.
/// </summary>
public sealed class ApiActions
{
    private readonly HttpClient _client;
    private readonly ApiContext _context;

    public ApiActions(HttpClient client, ApiContext context)
    {
        _client = client;
        _context = context;
    }

    public async Task<int> EnsureCategoryAsync(string name)
    {
        if (_context.CategoryIds.TryGetValue(name, out var existing))
            return existing;

        var id = await CreateCategoryAsync(name);
        _context.CategoryIds[name] = id;
        return id;
    }

    public async Task<int> CreateCategoryAsync(string name, string? description = null)
    {
        var response = await _client.PostAsJsonAsync("/api/categories", new { name, description });
        response.EnsureSuccessStatusCode();
        var id = await ExtractIdAsync(response);
        _context.CategoryIds[name] = id;
        return id;
    }

    public async Task<int> CreateProductAsync(string name, string? description, string categoryName)
    {
        await EnsureCategoryAsync(categoryName);
        var response = await _client.PostAsJsonAsync("/api/products", new { name, description, category = categoryName });
        response.EnsureSuccessStatusCode();
        var id = await ExtractIdAsync(response);
        _context.ProductIds[name] = id;
        return id;
    }

    public async Task DeleteProductByIdAsync(int id)
    {
        var response = await _client.DeleteAsync($"/api/products/{id}");
        response.EnsureSuccessStatusCode();
    }

    private static async Task<int> ExtractIdAsync(HttpResponseMessage response)
    {
        // El Location del 201 Created es la fuente fiable del Id asignado.
        var location = response.Headers.Location?.ToString() ?? "";
        if (int.TryParse(location.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault(), out var id))
            return id;

        // Respaldo: leer el "id" del body.
        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("id").GetInt32();
    }
}