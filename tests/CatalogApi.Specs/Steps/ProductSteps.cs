using System.Net;
using System.Net.Http.Json;
using CatalogApi.Specs.Infrastructure;
using CatalogApi.Specs.Support;
using Reqnroll;
using Shouldly;

namespace CatalogApi.Specs.Steps;

[Binding]
public sealed class ProductSteps
{
    private readonly HttpClient _client;
    private readonly ApiContext _context;
    private readonly ApiActions _actions;

    public ProductSteps(ScenarioContext scenario)
    {
        var runtime = new TestRuntime(scenario);
        _client = runtime.Client;
        _context = runtime.Context;
        _actions = runtime.Actions;
    }

    // -------------------------------- Dado --------------------------------

    [Given("que el catálogo no tiene productos")]
    public Task DadoQueElCatalogoNoTieneProductos()
    {
        TestFactory.ClearCatalog();
        return Task.CompletedTask;
    }

    [Given("que existe un producto llamado {string}")]
    [Given("existe un producto llamado {string}")]
    public async Task DadoExisteUnProductoLlamado(string nombre)
    {
        // El Antecedentes ya creó la categoría "Bebidas".
        await _actions.CreateProductAsync(nombre, null, "Bebidas");
    }

    [Given("que existe un producto llamado {string} en la categoría {string}")]
    public async Task DadoExisteUnProductoLlamadoEnLaCategoria(string nombre, string categoria)
    {
        await _actions.CreateProductAsync(nombre, null, categoria);
    }

    // -------------------------------- Cuando --------------------------------

    [When("creo un producto con los siguientes datos:")]
    public async Task CuandoCreoUnProductoConLosSiguientesDatos(DataTable table)
    {
        var data = ApiContext.ParseKeyValueTable(table);
        _context.LastEditName = data["Nombre"];
        data.TryGetValue("Descripción", out var description);
        _context.LastEditDescription = description;
        data.TryGetValue("Categoría", out var categoria);
        _context.LastEditCategory = categoria;

        var response = await _client.PostAsJsonAsync("/api/products",
            new { name = _context.LastEditName, description, category = categoria });
        await _context.CaptureAsync(response);
    }

    [When("intento crear un producto sin nombre")]
    public async Task CuandoIntentoCrearUnProductoSinNombre()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new { name = "", description = (string?)null, category = "Bebidas" });
        await _context.CaptureAsync(response);
    }

    [When("intento crear un producto sin categoría")]
    public async Task CuandoIntentoCrearUnProductoSinCategoria()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new { name = "Coca-Cola", description = (string?)null, category = (string?)null });
        await _context.CaptureAsync(response);
    }

    [When("intento crear un producto llamado {string}")]
    public async Task CuandoIntentoCrearUnProductoLlamado(string nombre)
    {
        var response = await _client.PostAsJsonAsync("/api/products", new { name = nombre, description = (string?)null, category = "Bebidas" });
        await _context.CaptureAsync(response);
    }

    [When("consulto el listado de productos")]
    public async Task CuandoConsultoElListadoDeProductos()
    {
        var response = await _client.GetAsync("/api/products");
        await _context.CaptureAsync(response);
    }

    [When("consulto el producto {string}")]
    public async Task CuandoConsultoElProducto(string nombre)
    {
        var id = _context.ProductIds[nombre];
        var response = await _client.GetAsync($"/api/products/{id}");
        await _context.CaptureAsync(response);
    }

    [When("edito el producto {string} con los siguientes datos:")]
    public async Task CuandoEditoElProductoConLosSiguientesDatos(string nombre, DataTable table)
    {
        var data = ApiContext.ParseKeyValueTable(table);
        _context.LastEditName = data["Nombre"];
        data.TryGetValue("Descripción", out var description);
        _context.LastEditDescription = description;
        data.TryGetValue("Categoría", out var categoria);
        _context.LastEditCategory = categoria;

        var id = _context.ProductIds[nombre];
        var response = await _client.PutAsJsonAsync($"/api/products/{id}",
            new { name = _context.LastEditName, description, category = categoria });
        await _context.CaptureAsync(response);
    }

    [When("intento editar el producto {string} sin nombre")]
    public async Task CuandoIntentoEditarElProductoSinNombre(string nombre)
    {
        var id = _context.ProductIds[nombre];
        var response = await _client.PutAsJsonAsync($"/api/products/{id}",
            new { name = "", description = (string?)null, category = "Bebidas" });
        await _context.CaptureAsync(response);
    }

    [When("intento renombrar el producto {string} a {string}")]
    public async Task CuandoIntentoRenombrarElProducto(string nombre, string nuevoNombre)
    {
        var id = _context.ProductIds[nombre];
        var response = await _client.PutAsJsonAsync($"/api/products/{id}",
            new { name = nuevoNombre, description = (string?)null, category = "Bebidas" });
        await _context.CaptureAsync(response);
    }

    [When("intento editar el producto {string} con los siguientes datos:")]
    public async Task CuandoIntentoEditarElProductoConLosSiguientesDatos(string nombre, DataTable table)
    {
        var data = ApiContext.ParseKeyValueTable(table);
        data.TryGetValue("Categoría", out var categoria);
        var id = _context.ProductIds.TryGetValue(nombre, out var foundId) ? foundId : int.MaxValue;
        var response = await _client.PutAsJsonAsync($"/api/products/{id}",
            new { name = data["Nombre"], description = (string?)null, category = categoria });
        await _context.CaptureAsync(response);
    }

    [When("elimino el producto {string}")]
    [When("intento eliminar el producto {string}")]
    public async Task CuandoEliminoElProducto(string nombre)
    {
        var id = _context.ProductIds.TryGetValue(nombre, out var foundId) ? foundId : int.MaxValue;
        var response = await _client.DeleteAsync($"/api/products/{id}");
        await _context.CaptureAsync(response);
    }

    // -------------------------------- Entonces --------------------------------

    [Then("el producto {string} queda registrado")]
    public void EntoncesElProductoQuedaRegistrado(string nombre)
    {
        _context.LastResponse!.StatusCode.ShouldBe(HttpStatusCode.Created);
        _context.LastJson!.Value.GetProperty("name").GetString().ShouldBe(nombre);
    }

    [Then("puedo consultarlo en el listado de productos")]
    public async Task EntoncesPuedoConsultarloEnElListadoDeProductos()
    {
        var response = await _client.GetAsync("/api/products");
        await _context.CaptureAsync(response);
        _context.LastList!
            .Select(e => e.GetProperty("name").GetString())
            .ShouldContain(_context.LastEditName);
    }

    [Then("veo el producto {string}")]
    public void EntoncesVeoElProducto(string nombre)
    {
        if (_context.LastList is not null)
        {
            _context.LastList
                .Select(e => e.GetProperty("name").GetString())
                .ShouldContain(nombre);
        }
        else
        {
            _context.LastJson!.Value.GetProperty("name").GetString().ShouldBe(nombre);
        }
    }

    [Then("veo el nombre de la categoría {string}")]
    public void EntoncesVeoElNombreDeLaCategoria(string nombre)
    {
        _context.LastJson!.Value.GetProperty("categoryName").GetString().ShouldBe(nombre);
    }

    [Then("el producto queda actualizado con el nuevo nombre, descripción y categoría")]
    public void EntoncesElProductoQuedaActualizado()
    {
        _context.LastJson!.Value.GetProperty("name").GetString().ShouldBe(_context.LastEditName);
        _context.LastJson!.Value.GetProperty("description").GetString().ShouldBe(_context.LastEditDescription);
        _context.LastJson!.Value.GetProperty("categoryName").GetString().ShouldBe(_context.LastEditCategory);
    }

    [Then("el producto queda en la categoría {string}")]
    public void EntoncesElProductoQuedaEnLaCategoria(string nombre)
    {
        _context.LastJson!.Value.GetProperty("categoryName").GetString().ShouldBe(nombre);
    }

    [Then("el producto queda eliminado del catálogo")]
    public void EntoncesElProductoQuedaEliminadoDelCatalogo()
    {
        _context.LastResponse!.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Then("no puedo consultarlo en el listado de productos")]
    public async Task EntoncesNoPuedoConsultarloEnElListadoDeProductos()
    {
        var response = await _client.GetAsync("/api/products");
        await _context.CaptureAsync(response);
        _context.LastList!.ShouldBeEmpty();
    }
}