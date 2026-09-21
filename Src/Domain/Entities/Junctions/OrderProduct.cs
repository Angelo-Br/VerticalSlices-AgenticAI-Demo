namespace VerticalSlicesDemo.Domain.Entities.Junctions;

public class OrderProduct : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Specific values for junction entity
    public required int Quantity { get; set; }
    
    public required decimal UnitPrice { get; set; }
}