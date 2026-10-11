using CatalogApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogApi.Specs.Infrastructure;

/// <summary>
/// Levanta la API real (Program) en un servidor de pruebas en memoria,
/// pero sobrescribe la cadena de conexión para que los escenarios NUNCA toquen
/// la base de desarrollo (catalog.db): usan catalog-tests.db.
/// </summary>
public sealed class CatalogApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            // Los providers agregados después tienen prioridad sobre appsettings.json.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Data Source=catalog-tests.db"
            });
        });
    }

    /// <summary>
    /// Asegura que el esquema exista en la base de pruebas aplicando las migraciones.
    /// El host es lazy: acceder a Services lo construye la primera vez.
    /// </summary>
    public void EnsureSchema()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        db.Database.Migrate();
    }

    /// <summary>
    /// Deja el catálogo vacío. El orden importa por la FK Restrict:
    /// primero productos, después categorías.
    /// </summary>
    public void ClearCatalog()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        db.Database.ExecuteSqlRaw("DELETE FROM Products; DELETE FROM Categories;");
    }
}