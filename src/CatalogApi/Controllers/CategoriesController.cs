using CatalogApi.Application;
using CatalogApi.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CatalogApi.Controllers;

[ApiController]
[Route("api/categories")]
public sealed class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryInput input, CancellationToken ct)
    {
        var category = await _service.CreateAsync(input.Name, input.Description, ct);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, ToDto(category));
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var categories = await _service.GetAllAsync(ct);
        return Ok(categories.Select(ToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken ct)
    {
        var category = await _service.GetByIdAsync(id, ct);
        return category is null ? NotFound() : Ok(ToDto(category));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CategoryInput input, CancellationToken ct)
    {
        var category = await _service.UpdateAsync(id, input.Name, input.Description, ct);
        return Ok(ToDto(category));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }

    private static CategoryDto ToDto(Category category) => new(category.Id, category.Name, category.Description);
}