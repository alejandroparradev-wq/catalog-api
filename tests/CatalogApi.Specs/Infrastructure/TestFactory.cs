namespace CatalogApi.Specs.Infrastructure;

/// <summary>
/// Estado global de la suite: un único factory (y un único catalog-tests.db)
/// para toda la corrida. Cada escenario se encarga de dejar el catálogo vacío.
/// </summary>
public static class TestFactory
{
    private static readonly CatalogApiFactory Factory = new();

    public static void EnsureSchema() => Factory.EnsureSchema();

    public static void ClearCatalog() => Factory.ClearCatalog();

    public static HttpClient CreateClient() => Factory.CreateClient();
}