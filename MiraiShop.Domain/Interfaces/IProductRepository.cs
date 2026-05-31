using MiraiShop.Domain.Entities;

namespace MiraiShop.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<IEnumerable<Product>> GetAllAsync();
    Task AddAsync(IEnumerable<Product> products);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
}