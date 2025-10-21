using Api.Data;
using Api.Models;
using Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Api.Repositories;

public class CustomerRepository(AppDbContext context, ILogger<CustomerRepository> logger) : ICustomerRepository
{
    private readonly AppDbContext _context = context;
    private readonly ILogger<CustomerRepository> _logger = logger;

    public async Task<Customer?> GetByIdAsync(long id)
    {
        try
        {
            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
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
            return await _context.Customers
                .FromSqlRaw(
                    "SELECT * FROM search_customers_by_name({0}, {1}, {2})",
                    term, limit, offset)
                .ToListAsync();
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
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
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
            var existing = await _context.Customers.FindAsync(customer.Id);
            if (existing == null)
                return null;

            existing.Name = customer.Name;
            existing.Document = customer.Document;
            existing.Active = customer.Active;

            await _context.SaveChangesAsync();
            return existing;
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
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return false;

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            throw;
        }
    }
}
