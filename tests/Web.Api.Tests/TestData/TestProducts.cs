using Web.Domain.Entities;

namespace Web.Api.Tests.TestData;

/// <summary>
///     Creates products for the tests. The times are fixed, so the responses can be compared exactly.
/// </summary>
internal static class TestProducts
{
    /// <summary>
    ///     The creation time of the test products.
    /// </summary>
    public static readonly DateTimeOffset Created = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);

    /// <summary>
    ///     Creates a stored product with the given values.
    /// </summary>
    public static Product Create(int id = 1, string name = "Test Product", decimal price = 10,
        string description = "desc")
    {
        return Product.Restore(id, name, "https://example.com/p.jpg", price, description, Created, Created);
    }
}
