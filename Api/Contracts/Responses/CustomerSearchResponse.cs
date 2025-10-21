using Api.Contracts.Common;
using Api.DTOs;

namespace Api.Contracts.Responses
{
    public class CustomerSearchResponse
    {
        public List<CustomerDto> Data { get; set; } = [];
        public PaginationInfo Pagination { get; set; } = new();
    }
}
