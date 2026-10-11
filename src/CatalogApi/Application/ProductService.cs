using CatalogApi.Data;
using CatalogApi.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Application;

public interface IProductService
{
    Task<Product> CreateAsync(string? name, string? description, string? categoryName, CancellationToken ct = default);
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Product> UpdateAsync(int id, string? name, string? description, string? categoryName, CancellationToken ct = default);
    Task<Product> DeleteAsync(int id, CancellationToken ct = default);
}

/// <summary>
/// Reglas de negocio de los productos:
///  - nombre obligatorio                             -> 400
///  - categoría obligatoria                          -> 400
///  - categoría inexistente (se referencia por nombre) -> 404
///  - nombre duplicado entre productos (CI)          -> 409
/// </summary>
public sealed class ProductService : IProductService
{
    private readonly CatalogDbContext _db;

    public ProductService(CatalogDbContext db) => _db = db;

    public async Task<Product> CreateAsync(string? name, string? description, string? categoryName, CancellationToken ct = default)
    {
        var trimmedName = RequireName(name);
        var categoryId = await ResolveCategoryAsync(categoryName, ct);
        await EnsureNameAvailableAsync(trimmedName, null, ct);

        var product = new Product(trimmedName, description, categoryId);
        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Name)
            .ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Product> UpdateAsync(int id, string? name, string? description, string? categoryName, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync(new object[] { id }, ct)
            ?? throw new DomainRuleException(StatusCodes.Status404NotFound, BusinessMessages.ProductNotFound);

        var trimmedName = RequireName(name);
        var categoryId = await ResolveCategoryAsync(categoryName, ct);
        await EnsureNameAvailableAsync(trimmedName, id, ct);

        product.Update(trimmedName, description, categoryId); // edición = reemplazo completo
        await _db.SaveChangesAsync(ct);
        return product;
    }

    public async Task<Product> DeleteAsync(int id, CancellationToken ct = default)
    {
        var product = await _db.Products.FindAsync(new object[] { id }, ct)
            ?? throw new DomainRuleException(StatusCodes.Status404NotFound, BusinessMessages.ProductNotFound);

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
        return product;
    }

    private static string RequireName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainRuleException(StatusCodes.Status400BadRequest, BusinessMessages.NameRequired);
        return name.Trim();
    }

    private async Task<int> ResolveCategoryAsync(string? categoryName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            throw new DomainRuleException(StatusCodes.Status400BadRequest, BusinessMessages.CategoryRequired);

        var trimmed = categoryName.Trim();
        var category = await _db.Categories.FirstOrDefaultAsync(
            c => c.Name.ToLower() == trimmed.ToLower(), ct);

        return category is null
            ? throw new DomainRuleException(StatusCodes.Status404NotFound, BusinessMessages.CategoryNotFound)
            : category.Id;
    }

    private async Task EnsureNameAvailableAsync(string name, int? exceptId, CancellationToken ct)
    {
        // Ver nota en CategoryService: ToLower() es traducible a SQLite (lower()).
        var duplicate = await _db.Products.AnyAsync(
            p => p.Name.ToLower() == name.ToLower()
                 && (exceptId == null || p.Id != exceptId),
            ct);

        if (duplicate)
            throw new DomainRuleException(StatusCodes.Status409Conflict, BusinessMessages.NameInUse);
    }
}