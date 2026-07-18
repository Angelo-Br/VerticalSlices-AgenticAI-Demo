using Microsoft.EntityFrameworkCore;
using VerticalSlicesDemo.Domain.Entities;

namespace VerticalSlicesDemo.Infrastructure.Databases;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{ 
    /// <summary>
    /// Add all db sets for the entities of the database.
    /// </summary>
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // https://learn.microsoft.com/en-us/ef/core/querying/filters
        // Use IgnoreQueryFilters() to ignore these filters
        // This can be improved using reflection instead of having to write every single entity here.
        modelBuilder.Entity<Order>().HasQueryFilter(c => c.DeletedAt == null);
    }
} 
