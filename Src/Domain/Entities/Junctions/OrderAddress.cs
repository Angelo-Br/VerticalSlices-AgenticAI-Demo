namespace VerticalSlicesDemo.Domain.Entities.Junctions;

public class OrderAddress : BaseEntity
{
    public Guid Id { get; set; }

    public required string StreetName { get; set; }
    
    public required string City { get; set; }
    
    public required string HouseNumber { get; set; }
    
    public string Addition { get; set; } = string.Empty;
    
    public required string PostalCode { get; set; }
    
    public required string Country { get; set; }
}