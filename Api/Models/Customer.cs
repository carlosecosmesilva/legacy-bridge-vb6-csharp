namespace Api.Models;

public class Customer
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Document { get; set; }
    public bool Status { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CustomerDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Document { get; set; }
    public bool Status { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public class CustomerSearchRequest
{
    public string Term { get; set; } = string.Empty;
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
}

public class CustomerSearchResponse
{
    public List<CustomerDto> Data { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
}

public class PaginationInfo
{
    public int Limit { get; set; }
    public int Offset { get; set; }
    public int Count { get; set; }
}
