using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Web.Application.Common;
using Web.Application.Common.Repositories;
using Web.Domain.Entities;
using Web.Infrastructure.Mock;

namespace Web.Infrastructure.Tests.Mock;

/// <summary>
///     Tests the in-memory product store used by the Mock data source.
/// </summary>
[TestFixture]
public class MockProductRepositoryTests
{
    private MockProductRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        FakeTimeProvider timeProvider = new(new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero));
        _repository = new(new Mock<ILogger<MockProductRepository>>().Object, timeProvider);
    }

    [Test]
    public async Task GetAllAsync_ReturnsSeedProducts()
    {
        IReadOnlyList<Product> products = await _repository.GetAllAsync(CancellationToken.None);

        Assert.That(products, Has.Count.EqualTo(ProductSeedData.CreateProducts(DateTimeOffset.UtcNow).Count));
    }

    [Test]
    public async Task GetPagedAsync_ReturnsRequestedPage()
    {
        PagedList<Product> page = await _repository.GetPagedAsync(1, 5, CancellationToken.None);

        Assert.That(page.Values, Has.Count.EqualTo(5));
        Assert.That(page.Values.First().Id, Is.EqualTo(6));
        Assert.That(page.TotalCount, Is.EqualTo(ProductSeedData.CreateProducts(DateTimeOffset.UtcNow).Count));
    }

    [Test]
    public async Task FindByIdAsync_WithSeedProduct_ReturnsProduct()
    {
        VersionedProduct? found = await _repository.FindByIdAsync(1, CancellationToken.None);

        Assert.That(found?.Product.Name, Is.EqualTo("iPhone 18 Pro"));
        Assert.That(found?.Version, Is.Not.Empty);
    }

    [Test]
    public async Task FindByIdAsync_WithUnknownId_ReturnsNull()
    {
        VersionedProduct? found = await _repository.FindByIdAsync(999, CancellationToken.None);

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task AddAsync_ReturnsTheStoredProductWithTheNextId()
    {
        Product product = new("New", "https://example.com/n.jpg", 1, "", DateTimeOffset.UtcNow);
        int seedCount = ProductSeedData.CreateProducts(DateTimeOffset.UtcNow).Count;

        Product stored = await _repository.AddAsync(product, CancellationToken.None);

        Assert.That(stored.Id, Is.EqualTo(seedCount + 1));
        Assert.That((await _repository.FindByIdAsync(stored.Id, CancellationToken.None))?.Product.Name,
            Is.EqualTo("New"));
    }

    [Test]
    public async Task UpdateAsync_StoresTheChangedDescriptionAndNewVersion()
    {
        VersionedProduct read = (await _repository.FindByIdAsync(2, CancellationToken.None))!;
        read.Product.ChangeDescription("Changed", DateTimeOffset.UtcNow);

        Product updated = await _repository.UpdateAsync(read, CancellationToken.None);

        Assert.That(updated.Description, Is.EqualTo("Changed"));
        VersionedProduct stored = (await _repository.FindByIdAsync(2, CancellationToken.None))!;
        Assert.That(stored.Product.Description, Is.EqualTo("Changed"));
        Assert.That(stored.Version, Is.Not.EqualTo(read.Version));
    }

    [Test]
    public async Task UpdateAsync_WithStaleCopy_ThrowsConflictAndKeepsTheSavedChange()
    {
        // Arrange: two readers load the same product, and the first one saves its change
        VersionedProduct first = (await _repository.FindByIdAsync(2, CancellationToken.None))!;
        VersionedProduct second = (await _repository.FindByIdAsync(2, CancellationToken.None))!;
        first.Product.ChangeDescription("First", DateTimeOffset.UtcNow);
        await _repository.UpdateAsync(first, CancellationToken.None);

        // Act
        second.Product.ChangeDescription("Second", DateTimeOffset.UtcNow);

        // Assert
        Assert.That(async () => await _repository.UpdateAsync(second, CancellationToken.None),
            Throws.TypeOf<ProductConflictException>());
        Assert.That((await _repository.FindByIdAsync(2, CancellationToken.None))?.Product.Description,
            Is.EqualTo("First"));
    }

    [Test]
    public void UpdateAsync_WhenTheProductIsMissing_ThrowsConflict()
    {
        // A new product has ID 0, and the store never assigns that ID
        Product product = new("Gone", "https://example.com/g.jpg", 1, "", DateTimeOffset.UtcNow);

        Assert.That(async () => await _repository.UpdateAsync(new(product, [1]), CancellationToken.None),
            Throws.TypeOf<ProductConflictException>());
    }

    [Test]
    public async Task FindByIdAsync_ReturnedCopy_DoesNotChangeStoredData()
    {
        Product copy = (await _repository.FindByIdAsync(1, CancellationToken.None))!.Product;
        copy.ChangeDescription("Changed outside the repository", DateTimeOffset.UtcNow);

        Product stored = (await _repository.FindByIdAsync(1, CancellationToken.None))!.Product;

        Assert.That(stored.Description, Is.Not.EqualTo("Changed outside the repository"));
    }
}
