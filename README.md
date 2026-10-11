# Catalog API

API de catálogo de productos y categorías, construida con ASP.NET Core (.NET 10), EF Core + SQLite y especificada con BDD (Reqnroll + Gherkin en español).

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
  El mensaje de negocio es: *"Esta categoria no se puede eliminar porque tiene productos asignados"* (literal del enunciado, sin tilde). En los escenarios se valida mediante el paso de negocio `se informa que la categoría tiene productos asignados`.

## Contrato HTTP

La API expone dos recursos. El nombre de la categoría se referencia **por su nombre** (cadena), no por su Id, tanto al crear como al editar productos.

| Método | Ruta                  | Éxito | Descripción |
|--------|-----------------------|-------|-------------|
| GET    | `/api/categories`     | 200   | Lista de categorías |
| GET    | `/api/categories/{id}`| 200   | Detalle de categoría |
| POST   | `/api/categories`     | 201   | Crea categoría (Locaion con su Id) |
| PUT    | `/api/categories/{id}`| 200   | Reemplaza la categoría |
| DELETE | `/api/categories/{id}`| 204   | Elimina la categoría |
| GET    | `/api/products`       | 200   | Lista de productos (incluye `categoryName`) |
| GET    | `/api/products/{id}`  | 200   | Detalle de producto |
| POST   | `/api/products`       | 201   | Crea producto |
| PUT    | `/api/products/{id}`  | 200   | Reemplaza el producto |
| DELETE | `/api/products/{id}`  | 204   | Elimina el producto |

**Cuerpos de entrada** (JSON):

```jsonc
// POST/PUT /api/categories
{ "name": "Bebidas", "description": "Refrescos y jugos" }

// POST/PUT /api/products
{ "name": "Coca-Cola", "description": "Refresco de cola", "category": "Bebidas" }
```

**Respuestas de error**: un objeto `{ "message": "..." }` con el código HTTP correspondiente:

| Código | Caso | Mensaje |
|--------|------|---------|
| 400    | Nombre vacío (categoría o producto) | `El nombre es obligatorio` |
| 400    | Producto sin categoría | `La categoría es obligatoria` |
| 404    | Categoría inexistente | `La categoría no existe` |
| 404    | Producto inexistente | `El producto no existe` |
| 409    | Nombre duplicado (case-insensitive) | `El nombre ya está en uso` |
| 409    | Eliminar categoría con productos | `Esta categoria no se puede eliminar porque tiene productos asignados` |

## Decisiones técnicas

- **Base de datos real: SQLite en archivo** (`catalog.db`), nunca en memoria, con migraciones EF Core. Al arrancar la API no se aplican migraciones automáticamente; el esquema se crea con `dotnet ef database update` (ver *Cómo correr*).
- **Unicidad case-insensitive con la base de datos como garantía.**
  El requisito de nombres únicos ignorando mayúsculas/minúsculas (aplica a categorías y, por decisión de negocio, también a productos) se implementa en dos capas:
  - **Validación en el servicio**: antes de crear o editar se verifica si ya existe el nombre (comparado en minúsculas). Si existe, la API responde con el mensaje de negocio y código 409 (Conflict).
  - **Índice único case-insensitive en la base de datos** (collation `NOCASE` de SQLite) como garantía de último recurso: la BD rechaza el duplicado aunque la validación en memoria no llegue a ejecutarse.
  - **Alcance documentado:** "ignorar mayúsculas/minúsculas" se interpreta sobre el rango ASCII (A-Z), que es lo que cubre `NOCASE`. Si más adelante se requiriera ignorar también acentos (p. ej. "Jugós" = "jugos"), la solución sería normalizar el nombre en una columna auxiliar antes del índice; hoy no es necesario porque el enunciado solo pide ignorar mayúsculas/minúsculas.
  - **Detalle de traducción a SQL:** la validación usa `ToLower()` (no `ToLowerInvariant()`): el provider de EF Core para SQLite traduce `ToLower()` a `lower()`, que es exactamente la semántica del índice `NOCASE`. `ToLowerInvariant()` **no es traducible** por EF Core y provocaría un error en tiempo de ejecución.
- **Reglas de negocio con `DomainRuleException`.**
  Los servicios de aplicación lanzan `DomainRuleException(statusCode, mensaje)` cuando se rompe una regla. Un middleware en `Program.cs` (registrado antes que los endpoints) la traduce a `{ "message": "..." }` con ese código. Esto mantiene los controladores delgados: solo traducen el resultado a HTTP.
- **Pruebas BDD como caja negra HTTP.**
  Los escenarios usan `WebApplicationFactory<Program>` (`Microsoft.AspNetCore.Mvc.Testing`) y atacan la API por HTTP real. Para que el proyecto de tests pueda referenciar a `Program` (que el compilador genera como clase interna parcial), se declara `public partial class Program { }` al final de `Program.cs`.
- **Base de datos aislada para las pruebas.**
  El factory sobrescribe la cadena de conexión con `Data Source=catalog-tests.db`; los tests **nunca** tocan `catalog.db` (la de desarrollo). El esquema se aplica una vez por corrida (`BeforeTestRun`) con `Database.Migrate()` sobre esa base de pruebas.
- **Aislamiento entre escenarios.**
  Un hook `BeforeScenario` deja las tablas vacías (`DELETE FROM Products; DELETE FROM Categories;` — en ese orden, por la FK con `Restrict`). El paralelismo de xUnit está desactivado (`[assembly: CollectionBehavior(DisableTestParallelization = true)]`) porque todas las features comparten la misma base SQLite en archivo; correr escenarios en paralelo produciría carreras de datos.
- **No se exponen entidades en JSON.** Las navegaciones (`Category.Products` / `Product.Category`) formarían ciclos de serialización; la API mapea a DTO (`CategoryDto`, `ProductDto`, `ProductInput`).

## Cómo correr

Requisitos: [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

```powershell
# 1) Restaurar y compilar
dotnet build

# 2) Crear la base de datos (aplica las migraciones a catalog.db)
dotnet ef database update --project src/CatalogApi

# 3) Levantar la API (perfil http: http://localhost:5149)
dotnet run --project src/CatalogApi --launch-profile http
```

Una vez corriendo, se puede probar con:

```powershell
# Crear una categoría
Invoke-RestMethod -Method Post -Uri "http://localhost:5149/api/categories" `
  -ContentType "application/json" -Body '{"name":"Bebidas","description":"Refrescos y jugos"}'

# Listar categorías
Invoke-RestMethod -Method Get -Uri "http://localhost:5149/api/categories"
```

## Cómo correr las pruebas

```powershell
dotnet test
```

La suite ejecuta los escenarios Gherkin de `tests/CatalogApi.Specs/Features/` (29 escenarios: 14 de categorías y 15 de productos) contra la API real en memoria, con su propia base de datos `catalog-tests.db`.

## Cómo desplegar (Docker)

La imagen multi-etapa (`Dockerfile`) compila la API y la corre con el runtime mínimo de ASP.NET. Requisitos: [Docker](https://www.docker.com/products/docker-desktop/).

```powershell
# 1) Generar el certificado autofirmado (una sola vez; no se commitea: está en .gitignore)
dotnet dev-certs https -ep .deploy/https/catalog-api.pfx -p "catalog-api"

# 2) Construir y levantar
docker compose up -d --build
```

Consumo:

| URL | Descripción |
|-----|-------------|
| `https://localhost:5443` | HTTPS con el certificado autofirmado (el navegador advertirá; aceptar es suficiente para la demo) |
| `http://localhost:8080` | HTTP plano (puerta para túneles externos) |

Para consumir por HTTPS desde PowerShell sin validar el certificado autofirmado:

```powershell
Invoke-RestMethod -Method Get -Uri "https://localhost:5443/api/categories" -SkipCertificateCheck
```

**Base de datos persistente.** `catalog.db` vive en el **volumen Docker** `catalog-data` (montado en `/app/data`); al reiniciar o reconstruir el contenedor (`docker compose down && up -d`) los datos siguen. La app **auto-migra al arranque** (`Migrate()` al iniciar), lo que evita tener que correr `dotnet ef database update` a mano en el contenedor. Si se prefiere el flujo manual: `Database:AutoMigrate=false` en las variables de entorno.

**Exponer la API a internet (opcional, ideal para el video).** Un túnel da una URL pública con HTTPS real sin configurar nada en la nube. Con [cloudflared](https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/downloads/) (gratis, sin cuenta para túneles rápidos):

```powershell
winget install --id Cloudflare.cloudflared
cloudflared tunnel --url http://localhost:8080
```

El comando imprime una URL `https://<nombre>.trycloudflare.com` apuntando a tu API, lista para compartir/consumir desde cualquier dispositivo.

**Decisiones de despliegue (para defender):**

- La app **no redirige** http->https internamente (`UseHttpsRedirection` no está): el TLS se termina en el edge (certificado en el contenedor o túnel). Redirigir dentro del contenedor causa bucles detrás de proxies/reverse proxies.
- El `docker publish` usa `-r linux-x64` porque **SQLite trae binarios nativos** (`e_sqlite3`) que deben publicarse con el RID del contenedor.
- `MapOpenApi()` solo se expone en `Development` (decisión de no documentar la API en producción). Para verlo en el despliegue, cambiar `if (app.Environment.IsDevelopment())` o lanzar el contenedor con `ASPNETCORE_ENVIRONMENT=Development`.

## Especificaciones BDD

Los escenarios de negocio viven en `tests/CatalogApi.Specs/Features/`:

- `Category.feature` — gestión de categorías.
- `Product.feature` — gestión de productos.

Cada feature se verifica de extremo a extremo:

- **Steps**: `tests/CatalogApi.Specs/Steps/` (pasos por recurso + pasos compartidos de validación).
- **Hooks**: `tests/CatalogApi.Specs/Hooks/` (ciclo de vida: esquema + limpieza).
- **Infraestructura**: `tests/CatalogApi.Specs/Infrastructure/` (factory que aísla la BD de pruebas).

## Uso de IA

Este proyecto se desarrolló con asistencia de un agente de IA (Big Pickle, vía OpenCode) bajo la siguiente dinámica:

- El estudiante decidió las reglas de negocio, validó el contrato y defendió cada decisión; la IA implementó el código, explicó el *porqué* de cada pieza y ejecutó las verificaciones.
- La IA revisó los archivos Gherkin, el esquema de migraciones y el código generado por Reqnroll antes de aceptarlo.
- Toda la implementación fue validada con `dotnet build` y `dotnet test`; ninguna dependencia se agregó sin verificar su API real (hubo que adaptar diferencias de Reqnroll 3.x, p. ej. `DataTableRow` en lugar de `TableRow`).
- El resumen ejecutivo para la defensa: 29/29 escenarios BDD verdes, persistencia verificada end-to-end (crear → reiniciar → los datos siguen) y despliegue Docker con HTTPS y volumen probado.