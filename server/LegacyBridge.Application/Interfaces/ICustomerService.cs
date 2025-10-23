using LegacyBridge.Application.Contracts.Requests;
using LegacyBridge.Application.Contracts.Responses;
using LegacyBridge.Application.DTOs;

namespace LegacyBridge.Application.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<CustomerDto?>> GetByIdAsync(long id);
    Task<ApiResponse<CustomerSearchResponse>> SearchByNameAsync(CustomerSearchRequest request);
    Task<ApiResponse<CustomerDto>> CreateAsync(CustomerDto customerDto);
    Task<ApiResponse<CustomerDto?>> UpdateAsync(long id, CustomerDto customerDto);
    Task<ApiResponse<bool>> DeleteAsync(long id);
}
