using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;

namespace VerticalSlicesDemo.Features.Orders;

public static class CancelOrder
{
    #region Request / Response
    private record Response(Guid OrderId);
    #endregion

    #region Endpoint and business logic
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("/customers/{customerId:Guid}/orders/{orderId:Guid}", Handler).WithTags("Orders");;
        }
        
        private static async Task<IResult> Handler(
            Guid customerId,
            Guid orderId)
        {
            return Results.Ok(new Response(Guid.NewGuid()));
        }
    }
    #endregion
}