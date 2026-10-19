using FluentResults;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Application.Tests.Products;

/// <summary>
///     Tests the creation of products from a command.
/// </summary>
[TestFixture]
public class ProductFactoryTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);

    [Test]
    public void Create_WithValidCommand_ReturnsProductWithTimestamps()
    {
        CreateProductCommand command = new()
        {
            Name = "Phone", ImgUri = "https://example.com/phone.jpg", Price = 99.99m, Description = "A phone"
        };

        Result<Product> result = ProductFactory.Create(command, CreatedAt);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("Phone"));
        Assert.That(result.Value.Price, Is.EqualTo(99.99m));
        Assert.That(result.Value.CreatedAt, Is.EqualTo(CreatedAt));
        Assert.That(result.Value.LastUpdatedAt, Is.EqualTo(CreatedAt));
    }

    [Test]
    public void Create_WithoutDescription_StoresEmptyDescription()
    {
        CreateProductCommand command = new() { Name = "Phone", ImgUri = "https://example.com/p.jpg", Price = 1 };

        Result<Product> result = ProductFactory.Create(command, CreatedAt);

        Assert.That(result.Value.Description, Is.Empty);
    }

    [Test]
    public void Create_WithPriceBelowZeroCent_Fails()
    {
        CreateProductCommand command = new() { Name = "Phone", ImgUri = "https://example.com/p.jpg", Price = 0.001m };

        Result<Product> result = ProductFactory.Create(command, CreatedAt);

        Assert.That(result.IsFailed, Is.True);
    }

    [Test]
    public void Create_WithUnsafeImageScheme_Fails()
    {
        CreateProductCommand command = new() { Name = "Phone", ImgUri = "javascript:alert(1)", Price = 1 };

        Result<Product> result = ProductFactory.Create(command, CreatedAt);

        Assert.That(result.IsFailed, Is.True);
    }
}
