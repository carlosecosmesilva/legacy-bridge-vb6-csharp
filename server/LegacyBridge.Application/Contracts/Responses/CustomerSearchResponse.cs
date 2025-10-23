using LegacyBridge.Application.Contracts.Common;
using LegacyBridge.Application.DTOs;

namespace LegacyBridge.Application.Contracts.Responses;

public class CustomerSearchResponse
{
    public List<CustomerDto> Data { get; set; } = [];
    public PaginationInfo Pagination { get; set; } = new();
}

