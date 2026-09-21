using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSlicesDemo.Infrastructure.Databases;
using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;

namespace VerticalSlicesDemo.Features.Customers;

public static class GetAllCustomers
{
    #region Response
    private record Response(List<CustomerResponse> Customers);

    private record CustomerResponse(
        Guid Id,
        string Name,
        string MiddleName,
        string LastName,
        string Email,
        bool IsEmailConfirmed,
        string PhoneNumber,
        DateOnly? DateOfBirth,
        List<AddressResponse> Addresses
    );

    private record AddressResponse(
        Guid Id,
        string StreetName,
        string City,
        string HouseNumber,
        string Addition,
        string PostalCode,
        string Country,
        bool IsStandardDeliveryAddress,
        bool IsStandardBillingAddress
    );
    #endregion

    #region Endpoint and business logic
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/customers", Handler).WithTags("Customers");
        }

        private static async Task<IResult> Handler(
            AppDbContext dbContext,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1)
                return Results.BadRequest("Page must be greater than 0");

            if (pageSize is < 1 or > 100)
                return Results.BadRequest("PageSize must be between 1 and 100");

            // Project instead of returning entities: Address.Customer points back at its owner,
            // so serializing the entity graph recurses until the response blows up.
            var customers = await dbContext.Customers
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CustomerResponse(
                    x.Id,
                    x.Name,
                    x.MiddleName,
                    x.LastName,
                    x.Email,
                    x.IsEmailConfirmed,
                    x.PhoneNumber,
                    x.DateOfBirth,
                    x.Addresses
                        .Select(a => new AddressResponse(
                            a.Id,
                            a.StreetName,
                            a.City,
                            a.HouseNumber,
                            a.Addition,
                            a.PostalCode,
                            a.Country,
                            a.IsStandardDeliveryAddress,
                            a.IsStandardBillingAddress))
                        .ToList()))
                .ToListAsync();

            return Results.Ok(new Response(customers));
        }
    }
    #endregion
}
