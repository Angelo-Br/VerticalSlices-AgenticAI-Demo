using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VerticalSlicesDemo.Domain.Entities;
using VerticalSlicesDemo.Domain.Entities.Junctions;
using VerticalSlicesDemo.Infrastructure.Databases;
using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;

namespace VerticalSlicesDemo.Features.Orders;

public static class CreateOrder
{
    #region Request / Response
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Request(
        List<OrderItemRequest> Products,
        AddressRequest BillingAddress,
        AddressRequest DeliveryAddress
    );

    // ReSharper disable once ClassNeverInstantiated.Global
    public record OrderItemRequest(
        Guid ProductId,
        int Quantity
    );

    // ReSharper disable once ClassNeverInstantiated.Global
    public record AddressRequest(
        string StreetName,
        string HouseNumber,
        string PostalCode,
        string City,
        string Country
    );
    
    private record Response(Guid OrderId);
    #endregion
    
    #region Request Validation
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Products)
                .NotEmpty()
                .WithMessage("At least one product is required.");

            RuleForEach(x => x.Products)
                .ChildRules(product =>
                {
                    product.RuleFor(x => x.ProductId)
                        .NotEmpty()
                        .WithMessage("Product id is required.");

                    product.RuleFor(x => x.Quantity)
                        .GreaterThan(0)
                        .WithMessage("Quantity must be greater than zero.");
                });

            RuleFor(x => x.BillingAddress)
                .NotNull()
                .ChildRules(address =>
                {
                    address.RuleFor(x => x.StreetName)
                        .NotEmpty();

                    address.RuleFor(x => x.HouseNumber)
                        .NotEmpty();

                    address.RuleFor(x => x.PostalCode)
                        .NotEmpty();

                    address.RuleFor(x => x.City)
                        .NotEmpty();

                    address.RuleFor(x => x.Country)
                        .NotEmpty();
                });

            RuleFor(x => x.DeliveryAddress)
                .NotNull()
                .ChildRules(address =>
                {
                    address.RuleFor(x => x.StreetName)
                        .NotEmpty();

                    address.RuleFor(x => x.HouseNumber)
                        .NotEmpty();

                    address.RuleFor(x => x.PostalCode)
                        .NotEmpty();

                    address.RuleFor(x => x.City)
                        .NotEmpty();

                    address.RuleFor(x => x.Country)
                        .NotEmpty();
                });
        }
    }
    #endregion

    #region Endpoint and business logic
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/customers/{customerId:Guid}/orders", Handler).WithTags("Orders");
        }
        
        private static async Task<IResult> Handler(
            AppDbContext dbContext,
            Guid customerId,
            Request body,
            IValidator<Request> validator
            )
        {
            // Validate incoming request
            var result = await validator.ValidateAsync(body);
            if (!result.IsValid)
            {
                return Results.BadRequest(result.Errors);
            }
            
            // Get the customer
            var customer = await dbContext.Customers.Where(x => x.Id == customerId).FirstOrDefaultAsync();
            if(customer == null)
            {
                return Results.NotFound();
            }
            
            // Get products
            var productIds = body.Products
                .Select(x => x.ProductId)
                .ToList();

            var products = await dbContext.Products
                .Where(x => productIds.Contains(x.Id))
                .ToListAsync();

            // Check all products exist
            if (products.Count != productIds.Count)
            {
                return Results.BadRequest("One or more products do not exist.");
            }
            
            var productLookup = products.ToDictionary(x => x.Id);

            // Calculate total price
            var totalPrice = body.Products.Sum(x =>
                productLookup[x.ProductId].Price * x.Quantity);
            
            // Mapping of a new order
            var newOrder = new Order
            {
                CustomerId = customer.Id,
                Customer = customer,
                TotalPrice = totalPrice, // calculate from products

                BillingAddress = new OrderAddress
                {
                    StreetName = body.BillingAddress.StreetName,
                    HouseNumber = body.BillingAddress.HouseNumber,
                    PostalCode = body.BillingAddress.PostalCode,
                    City = body.BillingAddress.City,
                    Country = body.BillingAddress.Country
                },

                DeliveryAddress = new OrderAddress
                {
                    StreetName = body.DeliveryAddress.StreetName,
                    HouseNumber = body.DeliveryAddress.HouseNumber,
                    PostalCode = body.DeliveryAddress.PostalCode,
                    City = body.DeliveryAddress.City,
                    Country = body.DeliveryAddress.Country
                },

                OrderProducts = body.Products.Select(p =>
                {
                    var product = productLookup[p.ProductId];

                    return new OrderProduct
                    {
                        ProductId = product.Id,
                        Quantity = p.Quantity,
                        UnitPrice = product.Price
                    };
                }).ToList()
            };
           
           // Add order to database
           dbContext.Orders.Add(newOrder);
           await dbContext.SaveChangesAsync();

           // Return results
           return Results.Created(
               $"/customers/{customerId}/orders/{newOrder.Id}",
               new Response(newOrder.Id)); 
        }
    }
    #endregion
}
