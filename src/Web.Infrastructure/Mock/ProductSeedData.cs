using Web.Domain.Entities;

namespace Web.Infrastructure.Mock;

/// <summary>
///     Sample products used to seed the database and the in-memory mock repository.
/// </summary>
public static class ProductSeedData
{
    /// <summary>
    ///     Creates a new list of sample products. Each call returns new instances, so callers can modify them freely.
    /// </summary>
    /// <param name="utcNow">The creation and update timestamp for every product.</param>
    public static List<Product> CreateProducts(DateTimeOffset utcNow)
    {
        return
        [
            new()
            {
                Name = "iPhone 18 Pro",
                ImgUri = "https://image-service.com/images/iphone18pro.jpg",
                Price = 48_900,
                Description = "The best Apple smartphone.",
                CreatedAt = utcNow,
                LastUpdatedAt = utcNow
            },
            new()
            {
                Name = "Samsung Galaxy S26",
                ImgUri = "https://image-service.com/images/galaxys26.jpg",
                Price = 29_000,
                Description = "Powerful Android smartphone with AI features",
                CreatedAt = utcNow,
                LastUpdatedAt = utcNow
            },
            new()
            {
                Name = "MacBook Air M5",
                ImgUri = "https://image-service.com/images/macbookair.jpg",
                Price = 59_900,
                Description = "Lightweight and powerful laptop with Apple M3 chip",
                CreatedAt = utcNow,
                LastUpdatedAt = utcNow
            },
            new()
            {
                Name = "Sony WH-1000XM5",
                ImgUri = "https://image-service.com/images/sony-headphones.jpg",
                Price = 6_900,
                Description = "Wireless noise-canceling headphones",
                CreatedAt = utcNow,
                LastUpdatedAt = utcNow
            },
            new()
            {
                Name = "iPad Pro 12.9",
                ImgUri = "https://image-service.com/images/ipadpro.jpg",
                Price = 23_900,
                Description = "Professional Apple tablet with Liquid Retina XDR display",
                CreatedAt = utcNow,
                LastUpdatedAt = utcNow
            },
            new()
            {
                Name = "Sony PlayStation 5 pro",
                ImgUri = "https://image-service.com/images/ps5pro.jpg",
                Price = 12_999,
                Description = "Next-generation game console",
                CreatedAt = utcNow,
                LastUpdatedAt = utcNow
            }
        ];
    }
}
