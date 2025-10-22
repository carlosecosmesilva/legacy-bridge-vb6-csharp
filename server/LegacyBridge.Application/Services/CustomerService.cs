using AutoMapper;
using Microsoft.Extensions.Logging;
using LegacyBridge.Application.Contracts.Requests;
using LegacyBridge.Application.Contracts.Responses;
using LegacyBridge.Application.DTOs;
using LegacyBridge.Application.Interfaces;
using LegacyBridge.Application.Contracts.Common;
using LegacyBridge.Domain.Entities;
using LegacyBridge.Domain.Interfaces.Repositories;

namespace LegacyBridge.Application.Services;

public class CustomerService(ICustomerRepository customerRepository, ILogger<CustomerService> logger, IMapper mapper) : ICustomerService
{
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly ILogger<CustomerService> _logger = logger;
    private readonly IMapper _mapper = mapper;

    public async Task<ApiResponse<CustomerDto?>> GetByIdAsync(long id)
    {
        try
        {
            if (id <= 0)
                return ApiResponse<CustomerDto?>.ErrorResult("Invalid customer ID");

            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
                return ApiResponse<CustomerDto?>.ErrorResult($"Customer {id} not found");


            var customerDto = _mapper.Map<CustomerDto>(customer);
            return ApiResponse<CustomerDto?>.SuccessResult(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer {CustomerId}", id);
            return ApiResponse<CustomerDto?>.ErrorResult("Internal server error");
        }
    }

    public async Task<ApiResponse<CustomerSearchResponse>> SearchByNameAsync(CustomerSearchRequest request)
    {
        try
        {
            if (request.Limit <= 0 || request.Limit > 1000)
                return ApiResponse<CustomerSearchResponse>.ErrorResult("Limit must be between 1 and 1000");

            if (request.Offset < 0)
                return ApiResponse<CustomerSearchResponse>.ErrorResult("Offset must be >= 0");

            var customers = await _customerRepository.SearchByNameAsync(request.Term, request.Limit, request.Offset);

            var customerDtos = _mapper.Map<List<CustomerDto>>(customers);

            var response = new CustomerSearchResponse
            {
                Data = customerDtos,
                Pagination = new PaginationInfo
                {
                    Limit = request.Limit,
                    Offset = request.Offset,
                    Count = customerDtos.Count
                }
            };

            _logger.LogInformation("Customer search executed: Term='{Term}', ResultCount={Count}",
                request.Term, customerDtos.Count);

            return ApiResponse<CustomerSearchResponse>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching customers with term: {Term}", request.Term);
            return ApiResponse<CustomerSearchResponse>.ErrorResult("Internal server error");
        }
    }

    public async Task<ApiResponse<CustomerDto>> CreateAsync(CustomerDto customerDto)
    {
        try
        {
            var (IsValid, Errors) = ValidateCustomer(customerDto);
            if (!IsValid)
                return ApiResponse<CustomerDto>.ErrorResult("Validation failed", Errors);


            var customer = _mapper.Map<Customer>(customerDto);
            var createdCustomer = await _customerRepository.CreateAsync(customer);
            var resultDto = _mapper.Map<CustomerDto>(createdCustomer);

            _logger.LogInformation("Customer created: {CustomerId}", createdCustomer.Id);
            return ApiResponse<CustomerDto>.SuccessResult(resultDto, "Customer created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer {CustomerName}", customerDto.Name);
            return ApiResponse<CustomerDto>.ErrorResult("Internal server error");
        }
    }

    public async Task<ApiResponse<CustomerDto?>> UpdateAsync(long id, CustomerDto customerDto)
    {
        try
        {
            if (id <= 0)
                return ApiResponse<CustomerDto?>.ErrorResult("Invalid customer ID");

            var (IsValid, Errors) = ValidateCustomer(customerDto);
            if (!IsValid)
                return ApiResponse<CustomerDto?>.ErrorResult("Validation failed", Errors);


            var customer = _mapper.Map<Customer>(customerDto);
            customer.Id = id;

            var updatedCustomer = await _customerRepository.UpdateAsync(customer);

            if (updatedCustomer == null)
                return ApiResponse<CustomerDto?>.ErrorResult($"Customer {id} not found");


            var resultDto = _mapper.Map<CustomerDto>(updatedCustomer);
            _logger.LogInformation("Customer updated: {CustomerId}", id);
            return ApiResponse<CustomerDto?>.SuccessResult(resultDto, "Customer updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return ApiResponse<CustomerDto?>.ErrorResult("Internal server error");
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id)
    {
        try
        {
            if (id <= 0)
                return ApiResponse<bool>.ErrorResult("Invalid customer ID");

            var deleted = await _customerRepository.DeleteAsync(id);

            if (!deleted)
                return ApiResponse<bool>.ErrorResult($"Customer {id} not found");


            _logger.LogInformation("Customer deleted: {CustomerId}", id);
            return ApiResponse<bool>.SuccessResult(true, "Customer deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            return ApiResponse<bool>.ErrorResult("Internal server error");
        }
    }



    private static (bool IsValid, List<string> Errors) ValidateCustomer(CustomerDto customer)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(customer.Name))
            errors.Add("Name is required");
        else if (customer.Name.Length > 255)
            errors.Add("Name must be less than 255 characters");

        if (!string.IsNullOrEmpty(customer.Document) && customer.Document.Length > 20)
            errors.Add("Document must be less than 20 characters");

        if (!customer.Status)
            errors.Add("Status is required");

        return (errors.Count == 0, errors);
    }
}
