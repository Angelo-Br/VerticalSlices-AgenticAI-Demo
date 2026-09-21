using Microsoft.EntityFrameworkCore;
using VerticalSlicesDemo.Domain.Entities;
using VerticalSlicesDemo.Domain.Entities.Junctions;

namespace VerticalSlicesDemo.Infrastructure.Databases;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{ 
    /// <summary>
    /// Add all db sets for the entities of the database.
    /// </summary>
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<OrderAddress> OrderAddresses { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // https://learn.microsoft.com/en-us/ef/core/querying/filters
        // Use IgnoreQueryFilters() to ignore these filters
        // This can be improved using reflection instead of having to write every single entity here.
        modelBuilder.Entity<Order>().HasQueryFilter(c => c.DeletedAt == null);
        modelBuilder.Entity<Customer>().HasQueryFilter(c => c.DeletedAt == null);
        modelBuilder.Entity<Address>().HasQueryFilter(c => c.DeletedAt == null);
        modelBuilder.Entity<Product>().HasQueryFilter(c => c.DeletedAt == null);
        modelBuilder.Entity<OrderAddress>().HasQueryFilter(c => c.DeletedAt == null);
        modelBuilder.Entity<OrderProduct>().HasQueryFilter(c => c.DeletedAt == null);
        
        // Entity configuration
        modelBuilder.Entity<Order>(builder =>
        {
            builder.HasOne(o => o.BillingAddress)
                .WithMany()
                .HasForeignKey(o => o.BillingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.DeliveryAddress)
                .WithMany()
                .HasForeignKey(o => o.DeliveryAddressId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<OrderProduct>(builder =>
        {
            // Composite PK
            builder.HasKey(op => new { op.OrderId, op.ProductId });

            // Order -> OrderProducts
            builder.HasOne(op => op.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(op => op.OrderId);

            // Product -> OrderProducts
            builder.HasOne(op => op.Product)
                .WithMany(p => p.OrderProducts)
                .HasForeignKey(op => op.ProductId);

            builder.Property(op => op.Quantity)
                .IsRequired();

            builder.Property(op => op.UnitPrice)
                .HasPrecision(18, 2);
        });
    }
} 
