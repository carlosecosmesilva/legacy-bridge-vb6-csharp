using Api.Models;
using Api.Repositories.Interfaces;
using Npgsql;

namespace Api.Repositories;

public class ProductRepository(IConfiguration configuration, ILogger<ProductRepository> logger) : IProductRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
    private readonly ILogger<ProductRepository> _logger = logger;

    public async Task<Product?> GetByIdAsync(long id)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT id, name, price, active, created_at FROM products WHERE id = @id",
                connection);
            command.Parameters.AddWithValue("id", id);

            using var reader = await command.ExecuteReaderAsync();
            
            if (await reader.ReadAsync())
            {
                return new Product
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Active = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                };
            }

            return null;
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
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT id, name, price, active, created_at FROM products ORDER BY name",
                connection);

            var products = new List<Product>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Active = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }

            return products;
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
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT id, name, price, active, created_at FROM products WHERE active = TRUE ORDER BY name",
                connection);

            var products = new List<Product>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(new Product
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Active = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                });
            }

            return products;
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
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"INSERT INTO products (name, price, active) 
                  VALUES (@name, @price, @active) 
                  RETURNING id, created_at",
                connection);
            command.Parameters.AddWithValue("name", product.Name);
            command.Parameters.AddWithValue("price", product.Price);
            command.Parameters.AddWithValue("active", product.Active);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                product.Id = reader.GetInt64(0);
                product.CreatedAt = reader.GetDateTime(1);
            }

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
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"UPDATE products 
                  SET name = @name, price = @price, active = @active 
                  WHERE id = @id",
                connection);
            command.Parameters.AddWithValue("id", product.Id);
            command.Parameters.AddWithValue("name", product.Name);
            command.Parameters.AddWithValue("price", product.Price);
            command.Parameters.AddWithValue("active", product.Active);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0 ? product : null;
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
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "DELETE FROM products WHERE id = @id",
                connection);
            command.Parameters.AddWithValue("id", id);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            throw;
        }
    }
}
