using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Web.Infrastructure.Mock;
using Web.Infrastructure.Persistence;

namespace Web.Infrastructure.Services;

/// <summary>
///     Applies database migrations and optionally adds example products.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    ///     How many times the migration is attempted before the connection failure is reported.
    /// </summary>
    private const int MIGRATION_ATTEMPTS = 5;

    /// <summary>
    ///     The pause between two migration attempts.
    /// </summary>
    private static readonly TimeSpan MigrationRetryDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     Applies pending migrations and, when requested, adds the example products.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="addProductExamples">Indicates whether to add example products.</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider, bool addProductExamples = true)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await MigrateWithRetryAsync(context);

        if (addProductExamples)
        {
            await AddProductExamplesAsync(context, scope.ServiceProvider.GetRequiredService<TimeProvider>());
        }
    }

    /// <summary>
    ///     Applies pending migrations without seeding data.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    public static async Task MigrateAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await MigrateWithRetryAsync(context);
    }

    /// <summary>
    ///     Applies pending migrations, retrying while the server is not accepting connections yet.
    /// </summary>
    /// <remarks>
    ///     A SQL Server container accepts TCP connections before the server is ready to log in, and a fresh start can
    ///     fail with a connection error. The connection error is not one that the EF Core retry policy handles, so the
    ///     migration is retried here for a short time. The last error is thrown when every attempt fails.
    /// </remarks>
    /// <param name="context">The database context.</param>
    private static async Task MigrateWithRetryAsync(DatabaseContext context)
    {
        for (int attempt = 1;; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                return;
            }
            catch (SqlException) when (attempt < MIGRATION_ATTEMPTS)
            {
                await Task.Delay(MigrationRetryDelay);
            }
        }
    }

    /// <summary>
    ///     Adds example products to the database if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    private static async Task AddProductExamplesAsync(DatabaseContext context, TimeProvider timeProvider)
    {
        if (context.Products.Any())
        {
            return;
        }

        context.Products.AddRange(ProductSeedData.CreateProducts(timeProvider.GetUtcNow()));
        await context.SaveChangesAsync();
    }
}
