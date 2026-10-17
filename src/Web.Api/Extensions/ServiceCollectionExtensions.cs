using FastEndpoints;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Web.Api.Configuration;
using Web.Infrastructure.Configurations;
using Web.Infrastructure.Services;

namespace Web.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddConfiguredOpenTelemetry()
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("shop-products-api"))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation()
                    .AddSqlClientInstrumentation())
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation())
                .UseOtlpExporter();

            return services;
        }
    }

    extension(WebApplicationBuilder builder)
    {
        public void AddPaginationOptions()
        {
            builder.Services.AddOptions<PaginationOptions>()
                .Bind(builder.Configuration.GetSection(PaginationOptions.SECTION_NAME))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        public string[] AddConfiguredCors()
        {
            string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

            if (allowedOrigins.Length > 0)
            {
                builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
                    policy.WithOrigins(allowedOrigins).WithMethods("GET", "POST", "PATCH").AllowAnyHeader()));
            }

            return allowedOrigins;
        }
    }

    extension(WebApplication app)
    {
        public void UseHealthChecksEndpoints()
        {
            // Liveness only confirms that the process responds. Readiness also checks the database.
            app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
            app.MapHealthChecks("/health/ready");
        }

        public void UseConfiguredFastEndpoints()
        {
            app.UseFastEndpoints(c =>
            {
                // Validation and other error responses use RFC 7807 problem details
                c.Errors.UseProblemDetails();
                c.Versioning.Prefix = "v";
                c.Versioning.PrependToRoute = true;
            });
        }

        public async Task HandleMigrationAsync(string[] args)
        {
            // Applies pending migrations and exits. Used as a separate deployment step.
            if (args.Contains("--migrate-only"))
            {
                if (Infrastructure.Extensions.ServiceCollectionExtensions.GetDataSource(app.Configuration) ==
                    DataSource.Database)
                {
                    await DatabaseInitializer.MigrateAsync(app.Services);
                    app.Logger.LogInformation("Database migrations are applied.");
                }
                else
                {
                    app.Logger.LogInformation("The Mock data source has no migrations to apply.");
                }

                Environment.Exit(0);
            }
        }
    }
}
