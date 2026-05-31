using Microsoft.EntityFrameworkCore;
using MiraiShop.Domain.Entities;
using MiraiShop.Domain.Exceptions;
using MiraiShop.Domain.Interfaces;
using MiraiShop.Infrastructure.Persistence;

namespace MiraiShop.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly MiraiShopDbContext _context;

    public ProductRepository(MiraiShopDbContext context)
    {
        _context = context;
    }

    public Task<Product?> GetByIdAsync(Guid id)
    {
        return Task.FromResult(new Product());
    }

    public Task<IEnumerable<Product>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(IEnumerable<Product> products)
    {
        var items = products.ToList();

        var codes = items.Select(p => p.CategoryCode).Distinct().ToList();
        var categoriesByCode = await _context.Categories
            .Where(c => codes.Contains(c.CategoryCode))
            .ToDictionaryAsync(c => c.CategoryCode);

        foreach (var product in items)
        {
            if (!categoriesByCode.TryGetValue(product.CategoryCode, out var category))
            {
                throw new CategoryNotFoundException(product.CategoryCode);
            }

            product.CategoryId = category.Id;
        }

        _context.Products.AddRange(items);
        await _context.SaveChangesAsync();
    }

    public Task UpdateAsync(Product product)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
