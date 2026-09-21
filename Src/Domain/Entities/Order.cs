using VerticalSlicesDemo.Domain.Entities.Junctions;

namespace VerticalSlicesDemo.Domain.Entities;

public class Order : BaseEntity
{
    public Guid Id { get; set; } 
    
    public required decimal TotalPrice { get; set; }
    
    // Fk Area
    public ICollection<OrderProduct> OrderProducts { get; set; } = [];

    public Guid BillingAddressId { get; set; }
    public required OrderAddress BillingAddress { get; set; } = null!;
    
    public Guid DeliveryAddressId { get; set; }
    public required OrderAddress DeliveryAddress { get; set; } = null!;
    
    public Guid CustomerId { get; set; }
    public required Customer Customer { get; set; } = null!;
}