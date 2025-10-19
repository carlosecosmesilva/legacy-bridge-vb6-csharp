using Api.Models;

namespace Api.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(long id);
    Task<List<Product>> GetAllAsync();
    Task<List<Product>> GetActiveAsync();
    Task<Product> CreateAsync(Product product);
    Task<Product?> UpdateAsync(Product product);
    Task<bool> DeleteAsync(long id);
}
