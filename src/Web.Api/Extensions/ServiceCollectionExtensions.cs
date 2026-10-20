using FastEndpoints;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Web.Api.Configuration;
using Web.Api.Errors;
using Web.Application.Products;
using Web.Infrastructure.Configurations;
using Web.Infrastructure.Services;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Web.Api.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Configures OpenTelemetry with tracing, metrics, and resource information for the application.
        /// Adds default instrumentation for ASP.NET Core and SQL Client, runtime metrics,
        /// and sets up the OTLP exporter for transmitting telemetry data.
        /// </summary>
        /// <returns>
        /// The configured <see cref="IServiceCollection"/>.
        /// </returns>
        public IServiceCollection AddConfiguredOpenTelemetry()
        {
            services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService("web-api"))
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
        /// <summary>
        ///     Registers the product use cases. The time source is registered by the infrastructure.
        /// </summary>
        public void AddProductServices()
        {
            builder.Services.AddScoped<IProductService, ProductService>();
        }

        public void AddPaginationOptions()
        {
            builder.Services.AddOptions<PaginationOptions>()
                .Bind(builder.Configuration.GetSection(PaginationOptions.SECTION_NAME))
                .ValidateDataAnnotations()
                .ValidateOnStart();
        }

        /// <summary>
        /// Configures CORS for the application by retrieving the allowed origins from the configuration
        /// and setting up a default policy with specified HTTP methods and headers.
        /// </summary>
        /// <returns>
        /// An array of allowed origins configured for CORS.
        /// </returns>
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
        /// <summary>
        /// Configures health check endpoints for the application.
        /// Adds a liveness endpoint at "/health/live" to verify the process is responsive
        /// and a readiness endpoint at "/health/ready" to confirm the application's dependencies are operational.
        /// </summary>
        public void UseHealthChecksEndpoints()
        {
            // Liveness only confirms that the process responds. Readiness also checks the database.
            app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
            app.MapHealthChecks("/health/ready");
        }

        /// <summary>
        /// Configures the application to use FastEndpoints with custom error response handling and versioning.
        /// Sets up a response builder for validation failures, which returns a <see cref="ProblemDetails"/> object
        /// containing details about the error, including a trace identifier, error messages, and other metadata.
        /// Configures endpoint versioning with a prefix and prepends it to the route for versioned APIs.
        /// </summary>
        public void UseConfiguredFastEndpoints()
        {
            app.UseFastEndpoints(c =>
            {
                c.Errors.ResponseBuilder = (failures, context, statusCode) =>
                {
                    ProblemDetails problemDetails = new()
                    {
                        Type = ProblemResults.BAD_REQUEST_TYPE,
                        Title = "Bad Request",
                        Status = statusCode,
                        Instance = context.Request.Path,
                        Detail = failures.FirstOrDefault()?.ErrorMessage,
                        Extensions =
                        {
                            ["traceId"] = context.TraceIdentifier,
                            ["errors"] = failures.Select(f => new
                            {
                                name = f.PropertyName, reason = f.ErrorMessage, code = f.ErrorCode
                            })
                        }
                    };

                    return problemDetails;
                };

                c.Versioning.Prefix = "v";
                c.Versioning.PrependToRoute = true;
            });
        }

        /// <summary>
        /// Handles the application migration process by applying pending database migrations
        /// if the "--migrate-only" argument is specified. If the data source is a database,
        /// it applies migrations and exits the application. For a mock data source, it simply logs
        /// that no migrations are required without making changes.
        /// </summary>
        /// <param name="args">The command-line arguments passed to the application.</param>
        /// <returns>An <see cref="Task"/> representing the asynchronous operation.</returns>
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
