using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSlicesDemo.Infrastructure.Databases;
using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;

namespace VerticalSlicesDemo.Features.Orders;

public static class GetAllOrdersFromCustomer
{
    #region Response
    private record Response(List<OrderResponse> Orders);

    private record OrderResponse(
        Guid Id,
        decimal TotalPrice,
        DateTime CreatedAt,
        AddressResponse BillingAddress,
        AddressResponse DeliveryAddress,
        List<OrderProductResponse> Products
    );

    private record OrderProductResponse(
        Guid ProductId,
        string Name,
        int Quantity,
        decimal UnitPrice
    );

    private record AddressResponse(
        string StreetName,
        string City,
        string HouseNumber,
        string Addition,
        string PostalCode,
        string Country
    );
    #endregion

    #region Endpoint and business logic
    // Endpoint class 
    public class Endpoint : IEndpoint
    {
        // To map the endpoint using minimal api's
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/customers/{customerId:guid}/orders", Handler).WithTags("Orders");;
        }
        
        // logic handling part of the endpoint, combined with response part of the minimal API endpoint
        private static async Task<IResult> Handler(
            AppDbContext dbContext,
            Guid customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1)
                return Results.BadRequest("Page must be greater than 0");

            if (pageSize is < 1 or > 100)
                return Results.BadRequest("PageSize must be between 1 and 100");

            // Project instead of returning entities: Product.OrderProducts points back at the order,
            // so serializing the entity graph recurses until the response blows up.
            var orders = await dbContext.Orders
                .Where(x => x.CustomerId == customerId)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrderResponse(
                    x.Id,
                    x.TotalPrice,
                    x.CreatedAt,
                    new AddressResponse(
                        x.BillingAddress.StreetName,
                        x.BillingAddress.City,
                        x.BillingAddress.HouseNumber,
                        x.BillingAddress.Addition,
                        x.BillingAddress.PostalCode,
                        x.BillingAddress.Country),
                    new AddressResponse(
                        x.DeliveryAddress.StreetName,
                        x.DeliveryAddress.City,
                        x.DeliveryAddress.HouseNumber,
                        x.DeliveryAddress.Addition,
                        x.DeliveryAddress.PostalCode,
                        x.DeliveryAddress.Country),
                    x.OrderProducts
                        .Select(op => new OrderProductResponse(
                            op.ProductId,
                            op.Product.Name,
                            op.Quantity,
                            op.UnitPrice))
                        .ToList()))
                .ToListAsync();

            return Results.Ok(new Response(orders));
        }
    }
    #endregion
}
