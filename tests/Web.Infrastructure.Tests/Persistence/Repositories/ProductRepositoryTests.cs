using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Web.Application.Common;
using Web.Application.Common.Repositories;
using Web.Domain.Entities;
using Web.Infrastructure.Persistence;
using Web.Infrastructure.Persistence.Repositories;

namespace Web.Infrastructure.Tests.Persistence.Repositories;

/// <summary>
///     Tests the storage behaviour of <see cref="ProductRepository" /> with the EF Core in-memory provider.
///     The business rules are tested in the Domain tests. Row-version conflicts need a real database, so they are
///     tested in <c>SqlServerProductRepositoryTests</c>.
/// </summary>
[TestFixture]
public class ProductRepositoryTests
{
    private static readonly DateTimeOffset Created = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private DatabaseContext _context = null!;
    private ProductRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new(options);
        _repository = new(_context, new Mock<ILogger<ProductRepository>>().Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task GetAllAsync_ReturnsProductsOrderedByCreationTimeAndId()
    {
        // Arrange
        _context.Products.AddRange(
            Record(new("Second", "https://example.com/2.jpg", 2, "", Created.AddHours(1))),
            Record(new("First", "https://example.com/1.jpg", 1, "", Created)));
        await _context.SaveChangesAsync();

        // Act
        IReadOnlyList<Product> products = await _repository.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.That(products.Select(p => p.Name), Is.EqualTo(new[] { "First", "Second" }));
    }

    [Test]
    public async Task GetAllAsync_WithEmptyDatabase_ReturnsEmptyList()
    {
        IReadOnlyList<Product> products = await _repository.GetAllAsync(CancellationToken.None);

        Assert.That(products, Is.Empty);
    }

    [Test]
    public async Task GetPagedAsync_ReturnsRequestedPageAndTotalCount()
    {
        // Arrange
        await AddProductsAsync(5);

        // Act
        PagedList<Product> page = await _repository.GetPagedAsync(1, 2, CancellationToken.None);

        // Assert
        Assert.That(page.TotalCount, Is.EqualTo(5));
        Assert.That(page.PageNumber, Is.EqualTo(1));
        Assert.That(page.PageSize, Is.EqualTo(2));
        Assert.That(page.Values.Select(p => p.Name), Is.EqualTo(new[] { "Product 3", "Product 4" }));
    }

    [Test]
    public async Task GetPagedAsync_PastTheLastPage_ReturnsEmptyPageWithTotalCount()
    {
        await AddProductsAsync(3);

        PagedList<Product> page = await _repository.GetPagedAsync(5, 2, CancellationToken.None);

        Assert.That(page.Values, Is.Empty);
        Assert.That(page.TotalCount, Is.EqualTo(3));
    }

    [Test]
    public async Task FindByIdAsync_WithExistingProduct_ReturnsProduct()
    {
        await AddProductsAsync(1);
        int id = _context.Products.Single().Id;

        VersionedProduct? found = await _repository.FindByIdAsync(id, CancellationToken.None);

        Assert.That(found?.Product.Name, Is.EqualTo("Product 1"));
    }

    [Test]
    public async Task FindByIdAsync_WithMissingProduct_ReturnsNull()
    {
        VersionedProduct? found = await _repository.FindByIdAsync(999, CancellationToken.None);

        Assert.That(found, Is.Null);
    }

    [Test]
    public async Task AddAsync_StoresProductAndReturnsItWithItsId()
    {
        // Arrange
        Product product = new("Phone", "https://example.com/phone.jpg", 10, "A phone", Created);

        // Act
        Product stored = await _repository.AddAsync(product, CancellationToken.None);

        // Assert
        Assert.That(stored.Id, Is.GreaterThan(0));
        ProductRecord record = await _context.Products.AsNoTracking().SingleAsync();
        Assert.That(record.Name, Is.EqualTo("Phone"));
        Assert.That(record.Description, Is.EqualTo("A phone"));
    }

    [Test]
    public async Task UpdateAsync_SavesTheChangedDescription()
    {
        // Arrange
        await AddProductsAsync(1);
        VersionedProduct read = (await _repository.FindByIdAsync(_context.Products.Single().Id,
            CancellationToken.None))!;
        DateTimeOffset updatedAt = Created.AddDays(1);
        read.Product.ChangeDescription("Changed", updatedAt);

        // Act
        Product updated = await _repository.UpdateAsync(read, CancellationToken.None);

        // Assert
        Assert.That(updated.Description, Is.EqualTo("Changed"));
        ProductRecord stored = await _context.Products.AsNoTracking().SingleAsync();
        Assert.That(stored.Description, Is.EqualTo("Changed"));
        Assert.That(stored.LastUpdatedAt, Is.EqualTo(updatedAt));
    }

    private static ProductRecord Record(Product product)
    {
        return ProductRecord.FromDomain(product, []);
    }

    private async Task AddProductsAsync(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            _context.Products.Add(Record(new($"Product {i}", "https://example.com/p.jpg", i, "", Created)));
        }

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();
    }
}
