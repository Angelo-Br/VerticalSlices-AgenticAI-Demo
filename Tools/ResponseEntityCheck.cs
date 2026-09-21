#:sdk Microsoft.NET.Sdk.Web
#:project ../VerticalSlicesDemo.csproj
#:property AllowMissingPrunePackageData=true

// Run with: dotnet run --no-cache Tools/ResponseEntityCheck.cs   (from outside the project directory)
// --no-cache matters: without it the runner reuses a stale build of the referenced project.
// Fails if a slice Response can reach a domain entity. Entities carry navigations back to their parent,
// so serializing one recurses forever and the endpoint dies mid-response with a 500.

using System.Reflection;
using VerticalSlicesDemo.Domain.Entities;
using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;

var responses = typeof(IEndpoint).Assembly
    .GetTypes()
    .Where(type => type.Name == "Response" && type.DeclaringType is not null)
    .ToArray();

if (responses.Length == 0)
{
    throw new Exception("Found no slice Response types, so this check is not looking where it thinks it is.");
}

var entityNamespace = typeof(BaseEntity).Namespace!;
var leaks = new List<string>();

foreach (var response in responses)
{
    foreach (var reached in Reachable(response))
    {
        if (reached.Namespace?.StartsWith(entityNamespace, StringComparison.Ordinal) == true)
        {
            leaks.Add($"{response.DeclaringType!.Name}.Response reaches entity {reached.Name}");
        }
    }

    Console.WriteLine($"{response.DeclaringType!.Name}.Response OK");
}

if (leaks.Count > 0)
{
    throw new Exception(string.Join(Environment.NewLine, leaks));
}

Console.WriteLine($"OK: {responses.Length} responses expose no entities.");
return;

static IEnumerable<Type> Reachable(Type root)
{
    var seen = new HashSet<Type>();
    var queue = new Queue<Type>([root]);

    while (queue.Count > 0)
    {
        var current = queue.Dequeue();
        var type = Nullable.GetUnderlyingType(current) ?? current;

        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
            {
                queue.Enqueue(argument);
            }
        }

        if (type.Namespace?.StartsWith("VerticalSlicesDemo", StringComparison.Ordinal) != true || !seen.Add(type))
        {
            continue;
        }

        yield return type;

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            queue.Enqueue(property.PropertyType);
        }
    }
}
