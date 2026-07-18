using VerticalSlicesDemo.Infrastructure.MinimalAPIReflection;
using Asp.Versioning;
using Asp.Versioning.Builder;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using VerticalSlicesDemo.Infrastructure.Databases;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
    })
    .AddOpenApi();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configure the database and interceptors.
builder.Services.AddSingleton<BaseEntityInterceptor>();
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options
        .UseInMemoryDatabase("db")
        .AddInterceptors(sp.GetRequiredService<BaseEntityInterceptor>());
});

builder.Services.AddProblemDetails();

// Build the app
var app = builder.Build();

// versioning
app.MapOpenApi().WithDocumentPerVersion();
app.MapScalarApiReference(options =>
{
    var descriptions = app.DescribeApiVersions();

    for (var i = 0; i < descriptions.Count; i++)
    {
        var description = descriptions[i];
        var isDefault = i == descriptions.Count - 1;

        // isDefault is used to mark the default API version in Scalar.
        // This decides which version is selected by default when users visit the Scalar UI.
        options.AddDocument(description.GroupName, description.GroupName, isDefault: isDefault);
    }
});

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{version:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.MapEndpoints(versionedGroup);

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cache-Control", "no-store");
    context.Response.Headers.Append("Content-Security-Policy", "frame-ancestors 'none'");
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");

    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Run();




