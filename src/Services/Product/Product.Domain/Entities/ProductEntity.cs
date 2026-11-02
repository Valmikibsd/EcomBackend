namespace Product.Domain.Entities;

public class ProductEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public string? Image { get; set; }
    public string? Colors { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
