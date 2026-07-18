using System.ComponentModel.DataAnnotations;

namespace VerticalSlicesDemo.Domain.Entities;

public class Order : BaseEntity
{
    [Key]
    public Guid Id { get; set; } 
    
    [MaxLength(250)]
    public required string ItemName { get; set; } 
    
    public decimal Price { get; set; }
}