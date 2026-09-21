using VerticalSlicesDemo.Domain.Entities.Junctions;

namespace VerticalSlicesDemo.Domain.Entities;

public class Product : BaseEntity
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required decimal Price { get; set; }

    public required int StockQuantity { get; set; }

    public string? SKU { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
    
    // Fk area
    public ICollection<OrderProduct> OrderProducts { get; set; } = [];
    
    //public Guid? CategoryId { get; set; }
    //public Category? Category { get; set; }
}