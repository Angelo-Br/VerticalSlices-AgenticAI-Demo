using System.ComponentModel.DataAnnotations;

namespace VerticalSlicesDemo.Domain.Entities;

public abstract class BaseEntity
{
    [Required]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    public DateTime UpdatedAt { get; set; }
    
    public DateTime? DeletedAt { get; set; }
}