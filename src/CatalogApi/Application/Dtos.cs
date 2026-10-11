namespace CatalogApi.Application;

/// <summary>Forma JSON de una categoría en las respuestas.</summary>
public sealed record CategoryDto(int Id, string Name, string? Description);

/// <summary>Forma JSON que recibe la API al crear/editar una categoría.</summary>
public sealed record CategoryInput(string? Name, string? Description);

/// <summary>
/// Forma JSON de un producto. Exponer el nombre de la categoría evita que el
/// cliente tenga que resolver la relación a mano.
/// </summary>
public sealed record ProductDto(int Id, string Name, string? Description, int CategoryId, string CategoryName);

/// <summary>
/// Forma JSON que recibe la API al crear/editar un producto.
/// La categoría llega por su <b>nombre</b> (cadena), no por su Id: es lo que el
/// contrato BDD usa y lo que un cliente humano puede escribir.
/// </summary>
public sealed record ProductInput(string? Name, string? Description, string? Category);