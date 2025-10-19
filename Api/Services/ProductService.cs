using Api.Models;
using Api.Repositories.Interfaces;
using Api.Services.Interfaces;

namespace Api.Services;

public class ProductService(IProductRepository productRepository, ILogger<ProductService> logger) : IProductService
{
    private readonly IProductRepository _productRepository = productRepository;
    private readonly ILogger<ProductService> _logger = logger;

    public async Task<ApiResponse<List<ProductDto>>> GetAllAsync()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            var productDtos = products.Select(MapToDto).ToList();

            _logger.LogInformation("All products retrieved: {Count} items", productDtos.Count);
            return ApiResponse<List<ProductDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all products");
            return ApiResponse<List<ProductDto>>.ErrorResult("Internal server error");
        }
    }

    public async Task<ApiResponse<List<ProductDto>>> GetActiveAsync()
    {
        try
        {
            var products = await _productRepository.GetActiveAsync();
            var productDtos = products.Select(MapToDto).ToList();

            _logger.LogInformation("Active products retrieved: {Count} items", productDtos.Count);
            return ApiResponse<List<ProductDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active products");
            return ApiResponse<List<ProductDto>>.ErrorResult("Internal server error");
        }
    }

    public async Task<ApiResponse<ProductDto?>> GetByIdAsync(long id)
    {
        try
        {
            if (id <= 0)
                return ApiResponse<ProductDto?>.ErrorResult("Invalid product ID");

            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
                return ApiResponse<ProductDto?>.ErrorResult($"Product {id} not found");

            var productDto = MapToDto(product);
            return ApiResponse<ProductDto?>.SuccessResult(productDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            return ApiResponse<ProductDto?>.ErrorResult("Internal server error");
        }
    }

    public async Task<ApiResponse<ProductDto>> CreateAsync(ProductDto productDto)
    {
        try
        {
            var (IsValid, Errors) = ValidateProduct(productDto);
            if (!IsValid)
                return ApiResponse<ProductDto>.ErrorResult("Validation failed", Errors);

            _logger.LogInformation("Creating product with name: {ProductName}", productDto.Name);

            var product = MapToEntity(productDto);
            var createdProduct = await _productRepository.CreateAsync(product);
            var resultDto = MapToDto(createdProduct);

            _logger.LogInformation("Product created: {ProductId}", createdProduct.Id);
            return ApiResponse<ProductDto>.SuccessResult(resultDto, "Product created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product {ProductName}", productDto.Name);
            return ApiResponse<ProductDto>.ErrorResult("Internal server error");
        }
    }

    public async Task<ApiResponse<ProductDto?>> UpdateAsync(long id, ProductDto productDto)
    {
        try
        {
            if (id <= 0)
            {
                return ApiResponse<ProductDto?>.ErrorResult("Invalid product ID");
            }

            var (IsValid, Errors) = ValidateProduct(productDto);
            if (!IsValid)
                return ApiResponse<ProductDto?>.ErrorResult("Validation failed", Errors);


            var product = MapToEntity(productDto);
            product.Id = id;

            _logger.LogInformation("Updating product with ID: {ProductId}", id);

            var updatedProduct = await _productRepository.UpdateAsync(product);

            if (updatedProduct == null)
                return ApiResponse<ProductDto?>.ErrorResult($"Product {id} not found");

            var resultDto = MapToDto(updatedProduct);
            _logger.LogInformation("Product updated: {ProductId}", id);
            return ApiResponse<ProductDto?>.SuccessResult(resultDto, "Product updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return ApiResponse<ProductDto?>.ErrorResult("Internal server error");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        try
        {
            if (id <= 0)
                return ApiResponse<bool>.ErrorResult("Invalid product ID");

            var deleted = await _productRepository.DeleteAsync(id);

            if (!deleted)
                return ApiResponse<bool>.ErrorResult($"Product {id} not found");

            _logger.LogInformation("Product deleted: {ProductId}", id);
            return ApiResponse<bool>.SuccessResult(true, "Product deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return ApiResponse<bool>.ErrorResult("Internal server error");
        }
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Active = product.Active,
            CreatedAt = product.CreatedAt
        };
    }

    private static Product MapToEntity(ProductDto productDto)
    {
        return new Product
        {
            Id = productDto.Id,
            Name = productDto.Name,
            Price = productDto.Price,
            Active = productDto.Active
        };
    }

    private static (bool IsValid, List<string> Errors) ValidateProduct(ProductDto product)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(product.Name))
            errors.Add("Name is required");
        else if (product.Name.Length > 255)
            errors.Add("Name must be less than 255 characters");

        if (product.Price < 0)
            errors.Add("Price must be greater than or equal to 0");

        return (errors.Count == 0, errors);
    }
}
