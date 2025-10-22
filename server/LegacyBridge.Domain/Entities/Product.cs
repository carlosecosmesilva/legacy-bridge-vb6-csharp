namespace LegacyBridge.Domain.Entities;

public class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Active { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}