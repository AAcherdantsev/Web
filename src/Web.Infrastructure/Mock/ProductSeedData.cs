using Web.Domain.Entities;

namespace Web.Infrastructure.Mock;

/// <summary>
///     Sample products used to fill an empty database and the mock data source.
/// </summary>
public static class ProductSeedData
{
    /// <summary>
    ///     Creates the sample products. They share one creation time, so the order between them comes from their IDs.
    /// </summary>
    /// <param name="utcNow">The creation time of the sample products.</param>
    public static List<Product> CreateProducts(DateTimeOffset utcNow)
    {
        return
        [
            new("iPhone 18 Pro", "https://image-service.com/images/iphone18pro.jpg", 48_900,
                "The best Apple smartphone.", utcNow),
            new("Samsung Galaxy S26", "https://image-service.com/images/galaxys26.jpg", 29_000,
                "Powerful Android smartphone with AI features", utcNow),
            new("MacBook Air M5", "https://image-service.com/images/macbookair.jpg", 59_900,
                "Lightweight and powerful laptop with Apple M5 chip", utcNow),
            new("Sony WH-1000XM5", "https://image-service.com/images/sony-headphones.jpg", 6_900,
                "Wireless noise-canceling headphones", utcNow),
            new("iPad Pro 12.9", "https://image-service.com/images/ipadpro.jpg", 23_900,
                "Professional Apple tablet with Liquid Retina XDR display", utcNow),
            new("Sony PlayStation 5 pro", "https://image-service.com/images/ps5pro.jpg", 12_999,
                "Next-generation game console", utcNow),
            new("Apple Watch Ultra 3", "https://image-service.com/images/watch-ultra.jpg", 8_990,
                "Rugged smartwatch for endurance sports and diving", utcNow),
            new("Samsung Galaxy Tab S11", "https://image-service.com/images/galaxy-tab-s11.jpg", 18_500,
                "Android tablet with an AMOLED display and a stylus", utcNow),
            new("Logitech MX Master 4", "https://image-service.com/images/mx-master-4.jpg", 1_290,
                "Ergonomic wireless mouse for productivity work", utcNow),
            new("Keychron Q6 Pro", "https://image-service.com/images/keychron-q6.jpg", 2_490,
                "Wireless mechanical keyboard with a full aluminium case", utcNow),
            new("Dell UltraSharp 27 4K", "https://image-service.com/images/dell-u2725.jpg", 15_900,
                "27-inch 4K monitor with USB-C power delivery", utcNow),
            new("Kindle Paperwhite", "https://image-service.com/images/kindle-paperwhite.jpg", 4_990,
                "E-reader with a glare-free display and a warm light", utcNow),
            new("GoPro HERO 14", "https://image-service.com/images/gopro-hero14.jpg", 4_490,
                "Waterproof action camera with 5.3K video", utcNow),
            new("Bose QuietComfort Ultra", "https://image-service.com/images/bose-qc-ultra.jpg", 9_990,
                "Over-ear headphones with spatial audio", utcNow),
            new("Nintendo Switch 2", "https://image-service.com/images/switch-2.jpg", 10_990,
                "Hybrid console that plays at home and on the go", utcNow),
            new("Garmin Forerunner 265", "https://image-service.com/images/garmin-265.jpg", 4_390,
                "Running watch with GPS and training load", utcNow),
            new("Anker 737 Power Bank", "https://image-service.com/images/anker-737.jpg", 1_590,
                "140 W power bank with two USB-C ports", utcNow)
        ];
    }
}
