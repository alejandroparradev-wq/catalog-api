using CatalogApi.Specs.Support;
using Reqnroll;
using Shouldly;

namespace CatalogApi.Specs.Steps;

/// <summary>
/// Pasos de validación compartidos por ambas features.
/// Los mensajes se verifican textual contra el contrato de la API.
/// </summary>
[Binding]
public sealed class ValidationSteps
{
    public const string NameRequired = "El nombre es obligatorio";
    public const string CategoryRequired = "La categoría es obligatoria";
    public const string NameInUse = "El nombre ya está en uso";
    public const string CategoryNotFound = "La categoría no existe";
    public const string ProductNotFound = "El producto no existe";
    public const string CategoryHasProducts = "Esta categoria no se puede eliminar porque tiene productos asignados";

    private readonly ApiContext _context;

    public ValidationSteps(ScenarioContext scenario)
    {
        _context = new TestRuntime(scenario).Context;
    }

    [Then("la creación es rechazada")]
    public void LaCreacionEsRechazada()
    {
        AssertClientError();
    }

    [Then("la edición es rechazada")]
    public void LaEdicionEsRechazada()
    {
        AssertClientError();
    }

    [Then("la eliminación es rechazada")]
    public void LaEliminacionEsRechazada()
    {
        AssertClientError();
    }

    [Then("se informa que el nombre es obligatorio")]
    public void SeInformaQueElNombreEsObligatorio()
    {
        _context.LastError.ShouldBe(NameRequired);
    }

    [Then("se informa que la categoría es obligatoria")]
    public void SeInformaQueLaCategoriaEsObligatoria()
    {
        _context.LastError.ShouldBe(CategoryRequired);
    }

    [Then("se informa que el nombre ya está en uso")]
    public void SeInformaQueElNombreYaEstaEnUso()
    {
        _context.LastError.ShouldBe(NameInUse);
    }

    [Then("se informa que la categoría no existe")]
    public void SeInformaQueLaCategoriaNoExiste()
    {
        _context.LastError.ShouldBe(CategoryNotFound);
    }

    [Then("se informa que el producto no existe")]
    public void SeInformaQueElProductoNoExiste()
    {
        _context.LastError.ShouldBe(ProductNotFound);
    }

    [Then("se informa que la categoría tiene productos asignados")]
    public void SeInformaQueLaCategoriaTieneProductosAsignados()
    {
        _context.LastError.ShouldBe(CategoryHasProducts);
    }

    private void AssertClientError()
    {
        var status = (int)_context.LastResponse!.StatusCode;
        status.ShouldBeInRange(400, 499);
    }
}