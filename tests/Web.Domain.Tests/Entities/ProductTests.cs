using Web.Domain.Entities;
using Web.Domain.Exceptions;

namespace Web.Domain.Tests.Entities;

/// <summary>
///     Tests that the product entity protects its own business rules, without any validator.
/// </summary>
[TestFixture]
public class ProductTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public void Constructor_WithValidValues_CreatesTheProduct()
    {
        Product product = new("Phone", "https://example.com/phone.jpg", 99.99m, "A phone", CreatedAt);

        Assert.That(product.Name, Is.EqualTo("Phone"));
        Assert.That(product.Price, Is.EqualTo(99.99m));
        Assert.That(product.Description, Is.EqualTo("A phone"));
        Assert.That(product.CreatedAt, Is.EqualTo(CreatedAt));
        Assert.That(product.LastUpdatedAt, Is.EqualTo(CreatedAt));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Constructor_WithEmptyName_Throws(string name)
    {
        Assert.That(() => new Product(name, "https://example.com/p.jpg", 1, "", CreatedAt),
            Throws.InstanceOf<InvalidProductException>()
                .With.Property(nameof(InvalidProductException.Violations))
                .Contains(ProductRules.NAME_EMPTY));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(0.001)]
    [TestCase(1.005)]
    [TestCase(10000000000000000)]
    public void Constructor_WithPriceOutsideTheRule_Throws(double price)
    {
        Assert.That(() => new Product("Phone", "https://example.com/p.jpg", (decimal) price, "", CreatedAt),
            Throws.TypeOf<InvalidProductException>());
    }

    [TestCase("javascript:alert(1)")]
    [TestCase("file:///etc/passwd")]
    [TestCase(null)]
    public void Constructor_WithImageAddressThatIsNotHttp_Throws(string? imgUri)
    {
        Assert.That(() => new Product("Phone", imgUri!, 1, "", CreatedAt),
            Throws.TypeOf<InvalidProductException>());
    }

    [Test]
    public void Constructor_WithDescriptionOverTheLimit_Throws()
    {
        string tooLong = new('x', ProductLimits.MAX_DESCRIPTION_LENGTH + 1);

        Assert.That(() => new Product("Phone", "https://example.com/p.jpg", 1, tooLong, CreatedAt),
            Throws.TypeOf<InvalidProductException>());
    }

    [Test]
    public void Constructor_ReportsEveryBrokenRule()
    {
        InvalidProductException exception = Assert.Throws<InvalidProductException>(() =>
            new Product("", "not a uri", 0, "", CreatedAt))!;

        Assert.That(exception.Violations, Is.EquivalentTo(new[]
        {
            ProductRules.NAME_EMPTY,
            ProductRules.PRICE_NOT_POSITIVE,
            ProductRules.IMG_URI_INVALID
        }));
    }

    [Test]
    public void ChangeDescription_WithValidText_ChangesTheDescriptionAndTheTime()
    {
        Product product = new("Phone", "https://example.com/p.jpg", 1, "old", CreatedAt);
        DateTimeOffset updatedAt = CreatedAt.AddDays(1);

        product.ChangeDescription("new", updatedAt);

        Assert.That(product.Description, Is.EqualTo("new"));
        Assert.That(product.LastUpdatedAt, Is.EqualTo(updatedAt));
    }

    [Test]
    public void ChangeDescription_OverTheLimit_ThrowsAndKeepsTheOldValues()
    {
        Product product = new("Phone", "https://example.com/p.jpg", 1, "old", CreatedAt);
        string tooLong = new('x', ProductLimits.MAX_DESCRIPTION_LENGTH + 1);

        Assert.That(() => product.ChangeDescription(tooLong, CreatedAt.AddDays(1)),
            Throws.TypeOf<InvalidProductException>());
        Assert.That(product.Description, Is.EqualTo("old"));
        Assert.That(product.LastUpdatedAt, Is.EqualTo(CreatedAt));
    }
}
