namespace Api.Models;

public class Customer
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Document { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}