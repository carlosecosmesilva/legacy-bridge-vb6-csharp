using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LegacyBridge.Domain.Entities;
using LegacyBridge.Domain.Interfaces.Repositories;
using LegacyBridge.Infrastructure.Persistence;

namespace LegacyBridge.Infrastructure.Repositories;

public class ProductRepository(AppDbContext context, ILogger<ProductRepository> logger) : IProductRepository
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<ProductRepository> _logger = logger;

    public async Task<Product?> GetByIdAsync(long id)
    {
        try
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            throw;
        }
    }

    public async Task<List<Product>> GetAllAsync()
    {
        try
        {
            return await _context.Products
                 .AsNoTracking()
                 .OrderBy(p => p.Name)
                 .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            throw;
        }
    }

    public async Task<List<Product>> GetActiveAsync()
    {
        try
        {
            return await _context.Products
                 .AsNoTracking()
                 .Where(p => p.Active)
                 .OrderBy(p => p.Name)
                 .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active products");
            throw;
        }
    }

    public async Task<Product> CreateAsync(Product product)
    {
        try
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {ProductName}", product.Name);
            throw;
        }
    }

    public async Task<Product?> UpdateAsync(Product product)
    {
        try
        {
            var existingProduct = await _context.Products.FindAsync(product.Id);
            if (existingProduct == null)
                return null;

            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            existingProduct.Active = product.Active;
            await _context.SaveChangesAsync();
            return existingProduct;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", product.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(long id)
    {
        try
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            throw;
        }
    }
}
