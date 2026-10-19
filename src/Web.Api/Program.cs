using FastEndpoints;
using FastEndpoints.Swagger;
using Mapster;
using Web.Api.Errors;
using Web.Api.Extensions;
using Web.Infrastructure.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddPaginationOptions();

TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
string[] allowedOrigins = builder.AddConfiguredCors();

builder.AddRepository();
builder.AddProductServices();
builder.AddApiRateLimiting();
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
{
    // The same members on every problem response, including the ones created by the framework and the handlers
    context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    context.ProblemDetails.Type = context.ProblemDetails.Status switch
    {
        StatusCodes.Status400BadRequest => ProblemResults.BAD_REQUEST_TYPE,
        StatusCodes.Status404NotFound => ProblemResults.NOT_FOUND_TYPE,
        _ => context.ProblemDetails.Type
    };
});
builder.Services.AddHealthChecks();
builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDocuments();
builder.Services.AddConfiguredOpenTelemetry();

WebApplication app = builder.Build();

await app.HandleMigrationAsync(args);

// Security headers come first, so they are also present on error responses
app.UseSecurityHeaders(app.Environment.IsDevelopment());
app.UseCustomExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

if (allowedOrigins.Length > 0)
{
    app.UseCors();
}

app.UseRateLimiter();
app.UseConfiguredFastEndpoints();
app.UseHealthChecksEndpoints();

// The API description is not published in production
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

app.Run();

// Make Program class accessible for integration tests
public partial class Program
{
}
