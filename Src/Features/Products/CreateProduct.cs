using FluentValidation;
using VerticalSlicesDemo.Domain.Entities;
using VerticalSlicesDemo.Infrastructure.Databases;
using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;

namespace VerticalSlicesDemo.Features.Products;

public static class CreateProduct
{
    #region Request / Response
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Request(
        string Name,
        string Description,
        decimal Price,
        int StockQuantity,
        string? SKU,
        string? ImageUrl
    );

    private record Response(Guid ProductId);
    #endregion
    
    #region Request Validation
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .NotEmpty();

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Price cannot be negative.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Stock quantity cannot be negative.");

            RuleFor(x => x.SKU)
                .MaximumLength(50)
                .When(x => x.SKU != null);

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .When(x => x.ImageUrl != null);
        }
    }
    #endregion

    #region Endpoint and business logic
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/products", Handler).WithTags("Products");
        }

        private static async Task<IResult> Handler(
            AppDbContext dbContext,
            Request body,
            IValidator<Request> validator)
        {
            // Validate incoming request
            var result = await validator.ValidateAsync(body);
            if (!result.IsValid)
            {
                return Results.BadRequest(result.Errors);
            }

            // Create product
            var newProduct = new Product
            {
                Name = body.Name,
                Description = body.Description,
                Price = body.Price,
                StockQuantity = body.StockQuantity,
                SKU = body.SKU,
                ImageUrl = body.ImageUrl,
                IsActive = true
            };

            // Add product to database
            dbContext.Products.Add(newProduct);
            await dbContext.SaveChangesAsync();

            // Return result
            return Results.Created(
                $"/products/{newProduct.Id}",
                new Response(newProduct.Id));
        }
    }
    #endregion
}