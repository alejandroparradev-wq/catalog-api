using System.Net;
using System.Net.Http.Json;
using CatalogApi.Specs.Infrastructure;
using CatalogApi.Specs.Support;
using Reqnroll;
using Shouldly;

namespace CatalogApi.Specs.Steps;

[Binding]
public sealed class CategorySteps
{
    private readonly HttpClient _client;
    private readonly ApiContext _context;
    private readonly ApiActions _actions;

    public CategorySteps(ScenarioContext scenario)
    {
        var runtime = new TestRuntime(scenario);
        _client = runtime.Client;
        _context = runtime.Context;
        _actions = runtime.Actions;
    }

    // -------------------------------- Dado --------------------------------

    [Given("que el catálogo no tiene categorías")]
    public Task DadoQueElCatalogoNoTieneCategorias()
    {
        TestFactory.ClearCatalog();
        return Task.CompletedTask;
    }

    [Given("que existe una categoría llamada {string}")]
    [Given("existe una categoría llamada {string}")]
    public async Task DadoExisteUnaCategoriaLlamada(string nombre)
    {
        await _actions.CreateCategoryAsync(nombre);
    }

    [Given("que existe una categoría llamada {string} con productos asignados")]
    public async Task DadoExisteUnaCategoriaConProductosAsignados(string nombre)
    {
        await _actions.CreateCategoryAsync(nombre);
        await _actions.CreateProductAsync("Producto de prueba", null, nombre);
    }

    [Given("que existía una categoría llamada {string} cuyo último producto fue eliminado")]
    public async Task DadoExistiaUnaCategoriaCuyoUltimoProductoFueEliminado(string nombre)
    {
        await _actions.CreateCategoryAsync(nombre);
        var productId = await _actions.CreateProductAsync("Producto de prueba", null, nombre);
        await _actions.DeleteProductByIdAsync(productId);
    }

    // -------------------------------- Cuando --------------------------------

    [When("creo una categoría con los siguientes datos:")]
    public async Task CuandoCreoUnaCategoriaConLosSiguientesDatos(DataTable table)
    {
        var data = ApiContext.ParseKeyValueTable(table);
        _context.LastEditName = data["Nombre"];
        data.TryGetValue("Descripción", out var description);
        _context.LastEditDescription = description;

        var response = await _client.PostAsJsonAsync("/api/categories", new { name = _context.LastEditName, description });
        await _context.CaptureAsync(response);
    }

    [When("intento crear una categoría sin nombre")]
    public async Task CuandoIntentoCrearUnaCategoriaSinNombre()
    {
        var response = await _client.PostAsJsonAsync("/api/categories", new { name = "", description = (string?)null });
        await _context.CaptureAsync(response);
    }

    [When("intento crear una categoría llamada {string}")]
    public async Task CuandoIntentoCrearUnaCategoriaLlamada(string nombre)
    {
        var response = await _client.PostAsJsonAsync("/api/categories", new { name = nombre, description = (string?)null });
        await _context.CaptureAsync(response);
    }

    [When("consulto el listado de categorías")]
    public async Task CuandoConsultoElListadoDeCategorias()
    {
        var response = await _client.GetAsync("/api/categories");
        await _context.CaptureAsync(response);
    }

    [When("consulto la categoría {string}")]
    public async Task CuandoConsultoLaCategoria(string nombre)
    {
        var id = _context.CategoryIds[nombre];
        var response = await _client.GetAsync($"/api/categories/{id}");
        await _context.CaptureAsync(response);
    }

    [When("edito la categoría {string} con los siguientes datos:")]
    public async Task CuandoEditoLaCategoriaConLosSiguientesDatos(string nombre, DataTable table)
    {
        var data = ApiContext.ParseKeyValueTable(table);
        _context.LastEditName = data["Nombre"];
        data.TryGetValue("Descripción", out var description);
        _context.LastEditDescription = description;

        var id = _context.CategoryIds[nombre];
        var response = await _client.PutAsJsonAsync($"/api/categories/{id}", new { name = _context.LastEditName, description });
        await _context.CaptureAsync(response);
    }

    [When("intento renombrar la categoría {string} a {string}")]
    public async Task CuandoIntentoRenombrarLaCategoria(string nombre, string nuevoNombre)
    {
        var id = _context.CategoryIds[nombre];
        var response = await _client.PutAsJsonAsync($"/api/categories/{id}", new { name = nuevoNombre, description = (string?)null });
        await _context.CaptureAsync(response);
    }

    [When("intento editar la categoría {string} sin nombre")]
    public async Task CuandoIntentoEditarLaCategoriaSinNombre(string nombre)
    {
        var id = _context.CategoryIds[nombre];
        var response = await _client.PutAsJsonAsync($"/api/categories/{id}", new { name = "", description = (string?)null });
        await _context.CaptureAsync(response);
    }

    [When("intento editar la categoría {string} con los siguientes datos:")]
    public async Task CuandoIntentoEditarLaCategoriaConLosSiguientesDatos(string nombre, DataTable table)
    {
        var data = ApiContext.ParseKeyValueTable(table);
        var id = _context.CategoryIds.TryGetValue(nombre, out var foundId) ? foundId : int.MaxValue;
        var response = await _client.PutAsJsonAsync($"/api/categories/{id}", new { name = data["Nombre"], description = (string?)null });
        await _context.CaptureAsync(response);
    }

    [When("elimino la categoría {string}")]
    [When("intento eliminar la categoría {string}")]
    public async Task CuandoEliminoLaCategoria(string nombre)
    {
        var id = _context.CategoryIds.TryGetValue(nombre, out var foundId) ? foundId : int.MaxValue;
        var response = await _client.DeleteAsync($"/api/categories/{id}");
        await _context.CaptureAsync(response);
    }

    // -------------------------------- Entonces --------------------------------

    [Then("la categoría {string} queda registrada")]
    public void EntoncesLaCategoriaQuedaRegistrada(string nombre)
    {
        _context.LastResponse!.StatusCode.ShouldBe(HttpStatusCode.Created);
        _context.LastJson!.Value.GetProperty("name").GetString().ShouldBe(nombre);
    }

    [Then("puedo consultarla en el listado de categorías")]
    public async Task EntoncesPuedoConsultarlaEnElListadoDeCategorias()
    {
        var response = await _client.GetAsync("/api/categories");
        await _context.CaptureAsync(response);
        _context.LastList!
            .Select(e => e.GetProperty("name").GetString())
            .ShouldContain(_context.LastEditName);
    }

    [Then("veo las categorías {string} y {string}")]
    public void EntoncesVeoLasCategorias(string primera, string segunda)
    {
        var names = _context.LastList!.Select(e => e.GetProperty("name").GetString()).ToList();
        names.ShouldContain(primera);
        names.ShouldContain(segunda);
    }

    [Then("veo el detalle de la categoría {string}")]
    public void EntoncesVeoElDetalleDeLaCategoria(string nombre)
    {
        _context.LastJson!.Value.GetProperty("name").GetString().ShouldBe(nombre);
    }

    [Then("la categoría queda actualizada con el nuevo nombre y descripción")]
    public void EntoncesLaCategoriaQuedaActualizada()
    {
        _context.LastJson!.Value.GetProperty("name").GetString().ShouldBe(_context.LastEditName);
        _context.LastJson!.Value.GetProperty("description").GetString().ShouldBe(_context.LastEditDescription);
    }

    [Then("la categoría queda eliminada")]
    public void EntoncesLaCategoriaQuedaEliminada()
    {
        _context.LastResponse!.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Then("no puedo consultarla en el listado de categorías")]
    public async Task EntoncesNoPuedoConsultarlaEnElListadoDeCategorias()
    {
        var response = await _client.GetAsync("/api/categories");
        await _context.CaptureAsync(response);
        _context.LastList!.ShouldBeEmpty();
    }
}