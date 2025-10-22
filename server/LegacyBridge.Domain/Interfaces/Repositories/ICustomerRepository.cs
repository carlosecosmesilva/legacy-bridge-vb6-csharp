using LegacyBridge.Domain.Entities;

namespace LegacyBridge.Domain.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(long id);
    Task<List<Customer>> SearchByNameAsync(string term, int limit, int offset);
    Task<Customer> CreateAsync(Customer customer);
    Task<Customer?> UpdateAsync(Customer customer);
    Task<bool> DeleteAsync(long id);
}
