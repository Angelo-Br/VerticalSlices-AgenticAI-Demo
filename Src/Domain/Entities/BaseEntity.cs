namespace VerticalSlicesDemo.Domain.Entities;

public abstract class BaseEntity
{
    public required DateTime CreatedAt { get; set; }
    
    public required DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
}