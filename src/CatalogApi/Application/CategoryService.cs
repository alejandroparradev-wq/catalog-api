using CatalogApi.Data;
using CatalogApi.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Application;

public interface ICategoryService
{
    Task<Category> CreateAsync(string? name, string? description, CancellationToken ct = default);
    Task<List<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Category> UpdateAsync(int id, string? name, string? description, CancellationToken ct = default);
    Task<Category> DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>
/// Reglas de negocio de las categorías. El controlador solo traduce el resultado
/// a HTTP; aquí vive el "porqué" de cada decisión:
///  - nombre obligatorio         -> 400
///  - nombre duplicado (CI)      -> 409
///  - borrar categoría con prod. -> 409 (mensaje literal del enunciado)
/// </summary>
public sealed class CategoryService : ICategoryService
{
    private readonly CatalogDbContext _db;

    public CategoryService(CatalogDbContext db) => _db = db;

    public async Task<Category> CreateAsync(string? name, string? description, CancellationToken ct = default)
    {
        var trimmedName = RequireName(name);
        await EnsureNameAvailableAsync(trimmedName, null, ct);

        var category = new Category(trimmedName, description);
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<List<Category>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Categories.FindAsync(new object[] { id }, ct);
    }

    public async Task<Category> UpdateAsync(int id, string? name, string? description, CancellationToken ct = default)
    {
        var category = await _db.Categories.FindAsync(new object[] { id }, ct)
            ?? throw new DomainRuleException(StatusCodes.Status404NotFound, BusinessMessages.CategoryNotFound);

        var trimmedName = RequireName(name);
        await EnsureNameAvailableAsync(trimmedName, id, ct);

        category.Update(trimmedName, description); // edición = reemplazo completo
        await _db.SaveChangesAsync(ct);
        return category;
    }

    public async Task<Category> DeleteAsync(int id, CancellationToken ct = default)
    {
        var category = await _db.Categories.FindAsync(new object[] { id }, ct)
            ?? throw new DomainRuleException(StatusCodes.Status404NotFound, BusinessMessages.CategoryNotFound);

        var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == id, ct);
        if (hasProducts)
            throw new DomainRuleException(StatusCodes.Status409Conflict, BusinessMessages.CategoryHasProducts);

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
        return category;
    }

    private static string RequireName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainRuleException(StatusCodes.Status400BadRequest, BusinessMessages.NameRequired);
        return name.Trim();
    }

    private async Task EnsureNameAvailableAsync(string name, int? exceptId, CancellationToken ct)
    {
        // ToLower() se traduce a lower() en SQLite (semántica ASCII, la misma del
        // índice COLLATE NOCASE). ToLowerInvariant() NO es traducible por EF Core.
        var duplicate = await _db.Categories.AnyAsync(
            c => c.Name.ToLower() == name.ToLower()
                 && (exceptId == null || c.Id != exceptId),
            ct);

        if (duplicate)
            throw new DomainRuleException(StatusCodes.Status409Conflict, BusinessMessages.NameInUse);
    }
}