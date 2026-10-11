using CatalogApi.Specs.Infrastructure;
using Reqnroll;

namespace CatalogApi.Specs.Hooks;

/// <summary>
/// Ciclo de vida de la suite:
///  - una vez por corrida: aplicar el esquema a catalog-tests.db;
///  - antes de cada escenario: dejar el catálogo vacío (el "Antecedentes" lo asume).
/// </summary>
[Binding]
public sealed class SuiteHooks
{
    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        TestFactory.EnsureSchema();
    }

    [BeforeScenario]
    public void ResetBetweenScenarios()
    {
        TestFactory.ClearCatalog();
    }
}