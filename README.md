# Catalog API

API de catálogo de productos y categorías, construida con ASP.NET Core y especificada con BDD (Reqnroll + Gherkin en español).

## Requerimientos funcionales

### Categorías

- El usuario puede **crear** categorías.
  - El nombre es obligatorio; la descripción es opcional.
  - El nombre no se puede repetir y la comparación ignora mayúsculas/minúsculas.
- El usuario puede **editar** una categoría (aplican las mismas reglas).
- El usuario puede **ver** las categorías: listado (INDEX) y detalle (SHOW).
- El usuario puede **eliminar** una categoría.
  - No se puede eliminar una categoría que tenga productos asignados.

### Productos

- El usuario puede **crear** productos.
  - El nombre es obligatorio, la descripción es opcional y la categoría es obligatoria.
- El usuario puede **editar** un producto, incluida su categoría.
- El usuario puede **ver** los productos, mostrando a qué categoría pertenecen (no solo el ID, también el nombre).
- El usuario puede **eliminar** productos.
  - Si se eliminan todos los productos de una categoría, ya se puede eliminar esa categoría.

## Decisiones de negocio

Estas decisiones complementan el enunciado y deben considerarse parte del contrato:

- **Unicidad del nombre de producto (case-insensitive).**
  El enunciado solo exige unicidad de nombre para categorías. Se decidió que el nombre de producto **también** sea único (ignorando mayúsculas/minúsculas), para evitar ambigüedad en el catálogo y ser coherente con la narrativa "organizar el catálogo y evitar duplicados". Aplica tanto a la creación como a la edición.

- **Edición como actualización completa.**
  Editar un producto o una categoría reemplaza el conjunto de campos editables (nombre, descripción y, en productos, categoría). Los escenarios de edición envían siempre el conjunto completo de campos para evitar ambigüedad entre actualización total y parcial.

- **Mensaje de eliminación de categoría.**
  El mensaje de negocio es: *"Esta categoría no se puede eliminar porque tiene productos asignados."* En los escenarios se valida mediante el paso de negocio `se informa que la categoría tiene productos asignados`.

## Especificaciones BDD

Los escenarios de negocio viven en `tests/CatalogApi.Specs/Features/`:

- `Category.feature` — gestión de categorías.
- `Product.feature` — gestión de productos.

Se ejecutan con:

```powershell
dotnet test
```
