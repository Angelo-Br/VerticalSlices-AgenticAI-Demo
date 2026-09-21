namespace VerticalSlicesDemo.Domain.Entities;

public class Customer : BaseEntity
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public string MiddleName { get; set; } = string.Empty;

    public required string LastName { get; set; }

    public required string Email { get; set; }

    public required bool IsEmailConfirmed { get; set; } = false;
    
    public string PhoneNumber { get; set; } = string.Empty;
    
    public DateOnly? DateOfBirth { get; set; }

    // FK Area
    public ICollection<Address> Addresses { get; set; } = [];
}