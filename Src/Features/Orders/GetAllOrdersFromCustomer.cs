using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSlicesDemo.Domain.Entities;
using VerticalSlicesDemo.Infrastructure.Databases;
using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;

namespace VerticalSlicesDemo.Features.Orders;

public static class GetAllOrdersFromCustomer
{
    private record Response(List<Order> Orders);

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

            var orders = await dbContext.Orders
                .Where(x => x.CustomerId == customerId)
                .Include(x => x.OrderProducts)
                .ThenInclude(x => x.Product)
                .Include(x => x.BillingAddress)
                .Include(x => x.DeliveryAddress)
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Results.Ok(new Response(orders));
        }
    }
}