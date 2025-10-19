using Api.Models;

namespace Api.Services.Interfaces;

public interface IProductService
{
    Task<ApiResponse<List<ProductDto>>> GetAllAsync();
    Task<ApiResponse<List<ProductDto>>> GetActiveAsync();
    Task<ApiResponse<ProductDto?>> GetByIdAsync(long id);
    Task<ApiResponse<ProductDto>> CreateAsync(ProductDto productDto);
    Task<ApiResponse<ProductDto?>> UpdateAsync(long id, ProductDto productDto);
    Task<ApiResponse<bool>> DeleteAsync(long id);
}
