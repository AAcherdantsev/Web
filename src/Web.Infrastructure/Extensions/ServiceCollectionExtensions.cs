using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Web.Application.Common.Repositories;
using Web.Infrastructure.Configurations;
using Web.Infrastructure.Mock;
using Web.Infrastructure.Persistence;
using Web.Infrastructure.Persistence.Repositories;
using Web.Infrastructure.Services;

namespace Web.Infrastructure.Extensions;

/// <summary>
///     Provides extension methods for configuring services in the application.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers the product repository for the data source selected by the <c>DataSource</c> setting.
    /// </summary>
    /// <remarks>
    ///     <c>Database</c> (default) uses SQL Server, with migrations and seed data controlled by <see cref="DatabaseOptions" />.
    ///     <c>Mock</c> uses an in-memory repository and needs no database. It is refused in the Production environment.
    /// </remarks>
    /// <param name="builder">The host application builder to configure services on.</param>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the <c>DataSource</c> value is unknown, when <c>Mock</c> is used in Production,
    ///     or when the <c>WebDb</c> connection string is missing.
    /// </exception>
    public static void AddRepository(this IHostApplicationBuilder builder)
    {
        DataSource dataSource = GetDataSource(builder.Configuration);

        builder.Services.TryAddSingleton(TimeProvider.System);

        if (dataSource == DataSource.Mock)
        {
            if (builder.Environment.IsProduction())
            {
                throw new InvalidOperationException(
                    "The Mock data source cannot be used in the Production environment. Set DataSource to Database.");
            }

            builder.Services.AddSingleton<IProductRepository, MockProductRepository>();
            return;
        }

        string? connectionString = builder.Configuration.GetConnectionString("WebDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "The connection string 'WebDb' is not configured. Set ConnectionStrings__WebDb, " +
                "or use DataSource=Mock for local development without a database.");
        }

        builder.Services.AddOptions<DatabaseOptions>()
            .Bind(builder.Configuration.GetSection(DatabaseOptions.SECTION_NAME));

        builder.Services.AddDbContext<DatabaseContext>(options =>
        {
            options
                .UseSqlServer(connectionString, o =>
                {
                    o.EnableRetryOnFailure(
                        5,
                        TimeSpan.FromSeconds(30),
                        null);
                    o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
        });

        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("database");
        builder.Services.AddHostedService<StartupInitializer>();
    }

    /// <summary>
    ///     Reads the <c>DataSource</c> setting. Defaults to <see cref="DataSource.Database" /> when it is not set.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public static DataSource GetDataSource(IConfiguration configuration)
    {
        string? value = configuration["DataSource"];

        if (string.IsNullOrWhiteSpace(value))
        {
            return DataSource.Database;
        }

        if (Enum.TryParse(value, true, out DataSource dataSource))
        {
            return dataSource;
        }

        throw new InvalidOperationException(
            $"Unknown DataSource '{value}'. Use '{nameof(DataSource.Database)}' or '{nameof(DataSource.Mock)}'.");
    }
}
