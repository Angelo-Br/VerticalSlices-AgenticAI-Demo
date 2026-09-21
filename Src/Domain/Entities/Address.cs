namespace VerticalSlicesDemo.Domain.Entities;

public class Address : BaseEntity
{
    public Guid Id { get; set; }

    public required string StreetName { get; set; }
    
    public required string City { get; set; }
    
    public required string HouseNumber { get; set; }
    
    public string Addition { get; set; } = string.Empty;
    
    public required string PostalCode { get; set; }
    
    public required string Country { get; set; }
    
    public required bool IsStandardDeliveryAddress { get; set; } = false;
    
    public required bool IsStandardBillingAddress { get; set; } = false;
    
    // Fk Area
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}