#:sdk Microsoft.NET.Sdk.Web
#:project ../VerticalSlicesDemo.csproj
#:property AllowMissingPrunePackageData=true
#:property JsonSerializerIsReflectionEnabledByDefault=true

// Run with: dotnet run --no-cache Tools/SchemaIdCheck.cs   (from outside the project directory)
// Fails if two slices would share one OpenAPI schema, which makes Scalar show the same body for every endpoint.

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using VerticalSlicesDemo.Features.Customers;
using VerticalSlicesDemo.Features.Orders;
using VerticalSlicesDemo.Features.Products;
using VerticalSlicesDemo.Infrastructure.OpenApi;

Type[] requests =
[
    typeof(CreateCustomer.Request),
    typeof(CreateCustomer.AddressRequest),
    typeof(CreateOrder.Request),
    typeof(CreateOrder.AddressRequest),
    typeof(CreateOrder.OrderItemRequest),
    typeof(CreateProduct.Request)
];

var jsonOptions = new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() };

var ids = requests
    .Select(type => SchemaIds.Qualified(jsonOptions.GetTypeInfo(type)))
    .ToArray();

foreach (var (type, id) in requests.Zip(ids))
{
    Console.WriteLine($"{type.DeclaringType!.Name}.{type.Name} -> {id}");
}

var duplicates = ids.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();
if (duplicates.Length > 0)
{
    throw new Exception($"Colliding OpenAPI schema ids: {string.Join(", ", duplicates)}");
}

Console.WriteLine($"OK: {ids.Length} request schemas have distinct ids.");
