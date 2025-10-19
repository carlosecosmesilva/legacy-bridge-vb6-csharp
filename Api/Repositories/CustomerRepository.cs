using Api.Models;
using Api.Repositories.Interfaces;
using Npgsql;

namespace Api.Repositories;

public class CustomerRepository(IConfiguration configuration, ILogger<CustomerRepository> logger) : ICustomerRepository
{
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
    private readonly ILogger<CustomerRepository> _logger = logger;

    public async Task<Customer?> GetByIdAsync(long id)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT id, name, document, status, created_at FROM customers WHERE id = @id",
                connection);
            command.Parameters.AddWithValue("id", id);

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Customer
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Document = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Status = reader.GetBoolean(3),
                    CreatedAt = reader.GetDateTime(4)
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
            throw;
        }
    }

    public async Task<List<Customer>> SearchByNameAsync(string term, int limit, int offset)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                "SELECT * FROM search_customers_by_name(@term, @limit, @offset)",
                connection);
            command.Parameters.AddWithValue("term", term);
            command.Parameters.AddWithValue("limit", limit);
            command.Parameters.AddWithValue("offset", offset);

            var customers = new List<Customer>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customers.Add(new Customer
                {
                    Id = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    Document = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Status = reader.GetBoolean(3)
                });
            }

            return customers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with term: {Term}", term);
            throw;
        }
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"INSERT INTO customers (name, document, status) 
                  VALUES (@name, @document, @status) 
                  RETURNING id, created_at",
                connection);
            command.Parameters.AddWithValue("name", customer.Name);
            command.Parameters.AddWithValue("document", (object?)customer.Document ?? DBNull.Value);
            command.Parameters.AddWithValue("status", customer.Status);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                customer.Id = reader.GetInt64(0);
                customer.CreatedAt = reader.GetDateTime(1);
            }

            return customer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer {CustomerName}", customer.Name);
            throw;
        }
    }

    public async Task<Customer?> UpdateAsync(Customer customer)
    {
        try
        {
            using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(
                @"UPDATE customers 
                  SET name = @name, document = @document, status = 0 
                  WHERE id = @id",
                connection);
            command.Parameters.AddWithValue("id", customer.Id);
            command.Parameters.AddWithValue("name", customer.Name);
            command.Parameters.AddWithValue("document", (object?)customer.Document ?? DBNull.Value);
            command.Parameters.AddWithValue("status", true);

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0 ? customer : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", customer.Id);
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
                "DELETE FROM customers WHERE id = @id",
                connection);
            command.Parameters.AddWithValue("id", id);
            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            throw;
        }
    }
}
