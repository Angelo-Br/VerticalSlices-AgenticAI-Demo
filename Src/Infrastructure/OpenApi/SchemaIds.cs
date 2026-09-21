using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.OpenApi;

namespace VerticalSlicesDemo.Infrastructure.OpenApi;

public static class SchemaIds
{
    /// <summary>
    /// Every slice nests its own <c>Request</c>/<c>AddressRequest</c>/<c>Response</c> record, and the default
    /// reference id is the bare type name, so all slices share one schema and every endpoint shows the same body.
    /// Prefixing with the declaring feature keeps them apart: <c>CreateProductRequest</c>, <c>CreateCustomerRequest</c>.
    /// </summary>
    public static string? Qualified(JsonTypeInfo typeInfo)
    {
        var id = OpenApiOptions.CreateDefaultSchemaReferenceId(typeInfo);

        return id is not null && typeInfo.Type.DeclaringType is { } feature ? feature.Name + id : id;
    }
}
