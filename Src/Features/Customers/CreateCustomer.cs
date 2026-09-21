using FluentValidation;
using VerticalSlicesDemo.Domain.Entities;
using VerticalSlicesDemo.Infrastructure.Databases;
using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;

namespace VerticalSlicesDemo.Features.Customers;

public static class CreateCustomer
{
    #region Request / Response
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Request(
        string Name,
        string MiddleName,
        string LastName,
        string Email,
        string PhoneNumber,
        DateOnly? DateOfBirth,
        AddressRequest Address
    );

    // ReSharper disable once ClassNeverInstantiated.Global
    public record AddressRequest(
        string StreetName,
        string City,
        string HouseNumber,
        string Addition,
        string PostalCode,
        string Country
    );
    
    private record Response(Guid CustomerId);
    #endregion
    
    #region Request Validation
    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Address)
                .NotNull();

            RuleFor(x => x.Address.StreetName)
                .NotEmpty();

            RuleFor(x => x.Address.City)
                .NotEmpty();

            RuleFor(x => x.Address.HouseNumber)
                .NotEmpty();

            RuleFor(x => x.Address.PostalCode)
                .NotEmpty();

            RuleFor(x => x.Address.Country)
                .NotEmpty();
        }
    }
    #endregion

    #region Endpoint and business logic
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("/customers", Handler).WithTags("Customers");
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

            // Create customer
            var newCustomer = new Customer
            {
                Name = body.Name,
                MiddleName = body.MiddleName,
                LastName = body.LastName,
                Email = body.Email,
                PhoneNumber = body.PhoneNumber,
                DateOfBirth = body.DateOfBirth,
                IsEmailConfirmed = false,

                Addresses =
                [
                    new Address
                    {
                        StreetName = body.Address.StreetName,
                        City = body.Address.City,
                        HouseNumber = body.Address.HouseNumber,
                        Addition = body.Address.Addition,
                        PostalCode = body.Address.PostalCode,
                        Country = body.Address.Country,

                        // Same address used for both
                        IsStandardDeliveryAddress = true,
                        IsStandardBillingAddress = true
                    }
                ]
            };

            // Add customer to database
            dbContext.Customers.Add(newCustomer);
            await dbContext.SaveChangesAsync();

            // Return results
            return Results.Created(
                $"/customers/{newCustomer.Id}",
                new Response(newCustomer.Id));
        }
    }
    #endregion
}