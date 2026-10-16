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
    ///     Applies pending migrations and, when requested, adds the example products.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="addProductExamples">Indicates whether to add example products.</param>
    public static async Task InitializeAsync(IServiceProvider serviceProvider, bool addProductExamples = true)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await context.Database.MigrateAsync();

        if (addProductExamples)
        {
            await AddProductExamplesAsync(context);
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
        await context.Database.MigrateAsync();
    }

    /// <summary>
    ///     Adds example products to the database if none exist.
    /// </summary>
    /// <param name="context">The database context.</param>
    private static async Task AddProductExamplesAsync(DatabaseContext context)
    {
        if (context.Products.Any())
        {
            return;
        }

        context.Products.AddRange(ProductSeedData.CreateProducts(DateTime.UtcNow));
        await context.SaveChangesAsync();
    }
}
