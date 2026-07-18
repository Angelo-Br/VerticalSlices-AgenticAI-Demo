using Microsoft.AspNetCore.Mvc;
using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;

namespace VerticalSlicesDemo.Features.Orders;

public static class GetAllOrders
{
    private record Response(Guid OrderId);

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
            Guid customerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1)
                return Results.BadRequest("Page must be greater than 0");

            if (pageSize is < 1 or > 100)
                return Results.BadRequest("PageSize must be between 1 and 100");

            return Results.Ok(new Response(Guid.NewGuid()));
        }
    }
}