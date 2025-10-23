namespace LegacyBridge.Application.Contracts.Requests;

public class CustomerSearchRequest
{
    public string Term { get; set; } = string.Empty;
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
}

