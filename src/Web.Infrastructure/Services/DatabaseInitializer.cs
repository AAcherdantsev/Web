using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        for (int attempt = 1; ; attempt++)
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
    /// <remarks>
    ///     Two instances that start at the same time must not both seed the table. The check and the insert run in one
    ///     transaction that takes an exclusive table lock, so the second instance waits, then finds the products. The
    ///     transaction runs inside the execution strategy, because the retry policy does not allow user transactions.
    /// </remarks>
    /// <param name="context">The database context.</param>
    /// <param name="timeProvider">The source of the creation time of the examples.</param>
    private static async Task AddProductExamplesAsync(DatabaseContext context, TimeProvider timeProvider)
    {
        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();

            bool hasProducts = await context.Products
                .FromSqlRaw("SELECT * FROM Products WITH (TABLOCKX, HOLDLOCK)")
                .AnyAsync();
            if (hasProducts)
            {
                return;
            }

            context.Products.AddRange(ProductSeedData.CreateProducts(timeProvider.GetUtcNow()));
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        });
    }
}
