namespace CatalogApi.Application;

/// <summary>
/// Mensajes de negocio de la API. Son el contrato entre la API y sus clientes:
/// los escenarios BDD los verifican literalmente, por eso viven en un solo lugar.
/// </summary>
public static class BusinessMessages
{
    public const string NameRequired = "El nombre es obligatorio";
    public const string CategoryRequired = "La categoría es obligatoria";
    public const string NameInUse = "El nombre ya está en uso";
    public const string CategoryNotFound = "La categoría no existe";
    public const string ProductNotFound = "El producto no existe";

    /// <summary>Literal del enunciado, sin tilde en "categoria".</summary>
    public const string CategoryHasProducts = "Esta categoria no se puede eliminar porque tiene productos asignados";
}