using FluentValidation;
using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;

namespace VerticalSlicesDemo.Features.Orders;

public static class CreateOrder
{
    #region Request / Response
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Request(
        List<OrderItem> Items,
        string ShippingAddress,
        string BillingAddress,
        string PaymentMethod,
        string? Notes,
        DateTime? RequestedDeliveryDate
    );
    // ReSharper disable once ClassNeverInstantiated.Global
    public record OrderItem(
        Guid ProductId,
        int Quantity
    );
    
    private record Response(Guid OrderId);
    #endregion
    
    #region Request Validation
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Items)
                .NotEmpty();

            RuleForEach(x => x.Items)
                .ChildRules(item =>
                {
                    item.RuleFor(x => x.ProductId)
                        .NotEmpty();

                    item.RuleFor(x => x.Quantity)
                        .GreaterThan(0);
                });

            RuleFor(x => x.ShippingAddress)
                .NotEmpty();

            RuleFor(x => x.BillingAddress)
                .NotEmpty();

            RuleFor(x => x.PaymentMethod)
                .NotEmpty();
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
            Guid customerId,
            Request body,
            IValidator<Request> validator
            )
        {
            var result = await validator.ValidateAsync(body);
            if (!result.IsValid)
            {
                return Results.BadRequest(result.Errors);
            }
            
            // Place order in database
            

            // Return results
            var orderId = Guid.NewGuid();
            return Results.Created(
                $"/customers/{customerId}/orders/{orderId}",
                new Response(orderId));
        }
    }
    #endregion
}
