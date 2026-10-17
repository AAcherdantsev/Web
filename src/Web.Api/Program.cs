using FastEndpoints;
using FastEndpoints.Swagger;
using Mapster;
using Web.Api.Extensions;
using Web.Infrastructure.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddPaginationOptions();

TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);
string[] allowedOrigins = builder.AddConfiguredCors();

builder.AddRepository();
builder.AddApiRateLimiting();
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
