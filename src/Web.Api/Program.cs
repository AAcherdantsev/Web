using FastEndpoints;
using FastEndpoints.Swagger;
using Mapster;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Web.Api.Configuration;
using Web.Api.Extensions;
using Web.Infrastructure.Configurations;
using Web.Infrastructure.Extensions;
using Web.Infrastructure.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<PaginationOptions>()
    .Bind(builder.Configuration.GetSection(PaginationOptions.SECTION_NAME))
    .ValidateDataAnnotations()
    .ValidateOnStart();

TypeAdapterConfig.GlobalSettings.Scan(typeof(Program).Assembly);

string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (allowedOrigins.Length > 0)
{
    builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).WithMethods("GET", "POST", "PATCH").AllowAnyHeader()));
}

builder.Services.AddFastEndpoints();
builder.Services.AddSwaggerDocuments();
builder.Services.AddHealthChecks();
builder.AddRepository();
builder.AddApiRateLimiting();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("shop-products-api"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSqlClientInstrumentation())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddRuntimeInstrumentation())
    .UseOtlpExporter();

WebApplication app = builder.Build();

// Applies pending migrations and exits. Used as a separate deployment step.
if (args.Contains("--migrate-only"))
{
    if (ServiceCollectionExtensions.GetDataSource(app.Configuration) == DataSource.Database)
    {
        await DatabaseInitializer.MigrateAsync(app.Services);
        app.Logger.LogInformation("Database migrations are applied.");
    }
    else
    {
        app.Logger.LogInformation("The Mock data source has no migrations to apply.");
    }

    return;
}

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
app.UseFastEndpoints(c =>
{
    // Validation and other error responses use RFC 7807 problem details
    c.Errors.UseProblemDetails();
    c.Versioning.Prefix = "v";
    c.Versioning.PrependToRoute = true;
});

// Liveness only confirms that the process responds. Readiness also checks the database.
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

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
