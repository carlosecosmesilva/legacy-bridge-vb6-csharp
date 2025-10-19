using Api.Models;

namespace Api.Services.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<CustomerDto?>> GetByIdAsync(long id);
    Task<ApiResponse<CustomerSearchResponse>> SearchByNameAsync(CustomerSearchRequest request);
    Task<ApiResponse<CustomerDto>> CreateAsync(CustomerDto customerDto);
    Task<ApiResponse<CustomerDto?>> UpdateAsync(long id, CustomerDto customerDto);
    Task<ApiResponse<bool>> DeleteAsync(long id);
}
