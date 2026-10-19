using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Web.Application.Common;
using Web.Application.Common.Errors;
using Web.Application.Products;
using Web.Domain.Entities;
using Web.Infrastructure.Mock;

namespace Web.Infrastructure.Tests.Mock;

/// <summary>
///     Unit tests for the in-memory <see cref="MockProductRepository" />.
/// </summary>
[TestFixture]
public class MockProductRepositoryTests
{
    [SetUp]
    public void SetUp()
    {
        _repository = new(new Mock<ILogger<MockProductRepository>>().Object);
    }

    private MockProductRepository _repository;

    [Test]
    public async Task GetAllProductsAsync_ReturnsSeedData()
    {
        // Act
        Result<IEnumerable<Product>> result = await _repository.GetAllProductsAsync(CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Count(), Is.EqualTo(ProductSeedData.CreateProducts(DateTimeOffset.UtcNow).Count));
    }

    [Test]
    public async Task GetAllProductsAsync_WithPagination_ReturnsRequestedPage()
    {
        // Arrange
        GetPagedProductsQuery query = new() { PageNumber = 1, PageSize = 4 };

        // Act
        Result<PagedList<Product>> result = await _repository.GetAllProductsAsync(query, CancellationToken.None);

        // Assert
        int total = ProductSeedData.CreateProducts(DateTimeOffset.UtcNow).Count;
        Assert.That(result.Value.TotalCount, Is.EqualTo(total));
        Assert.That(result.Value.Values, Has.Count.EqualTo(total - 4));
    }

    [Test]
    public async Task GetProductByIdAsync_WithSeedProduct_ReturnsProduct()
    {
        // Act
        Result<Product> result = await _repository.GetProductByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(1));
    }

    [Test]
    public async Task GetProductByIdAsync_WithUnknownId_ReturnsNotFoundError()
    {
        // Act
        Result<Product> result = await _repository.GetProductByIdAsync(999, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Errors.First(), Is.TypeOf<NotFoundError>());
    }

    [Test]
    public async Task CreateProductAsync_AssignsNextIdAndStoresProduct()
    {
        // Arrange
        CreateProductCommand command = new()
        {
            Name = "Mock Product",
            ImgUri = "https://example.com/mock.jpg",
            Price = 5
        };

        // Act
        Result<Product> created = await _repository.CreateProductAsync(command, CancellationToken.None);
        Result<Product> fetched = await _repository.GetProductByIdAsync(created.Value.Id, CancellationToken.None);

        // Assert
        Assert.That(created.IsSuccess, Is.True);
        Assert.That(created.Value.Id, Is.GreaterThan(ProductSeedData.CreateProducts(DateTimeOffset.UtcNow).Count));
        Assert.That(fetched.Value.Name, Is.EqualTo("Mock Product"));
    }

    [Test]
    public async Task CreateProductAsync_WithInvalidCommand_ReturnsFailure()
    {
        // Arrange
        CreateProductCommand command = new() { Name = "", ImgUri = "https://example.com/mock.jpg", Price = 5 };

        // Act
        Result<Product> result = await _repository.CreateProductAsync(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Errors.First().Message, Is.EqualTo("Name cannot be empty."));
    }

    [Test]
    public async Task UpdateProductDescriptionAsync_UpdatesStoredProduct()
    {
        // Act
        Result<Product> updated = await _repository.UpdateProductDescriptionAsync(2, "Changed", CancellationToken.None);
        Result<Product> fetched = await _repository.GetProductByIdAsync(2, CancellationToken.None);

        // Assert
        Assert.That(updated.IsSuccess, Is.True);
        Assert.That(fetched.Value.Description, Is.EqualTo("Changed"));
    }

    [Test]
    public async Task UpdateProductDescriptionAsync_WithUnknownId_ReturnsNotFoundError()
    {
        // Act
        Result<Product> result = await _repository.UpdateProductDescriptionAsync(999, "text", CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Errors.First(), Is.TypeOf<NotFoundError>());
    }

    [Test]
    public async Task GetProductByIdAsync_ReturnedCopy_DoesNotChangeStoredData()
    {
        // Arrange
        Result<Product> first = await _repository.GetProductByIdAsync(1, CancellationToken.None);
        first.Value.Name = "Changed outside the repository";

        // Act
        Result<Product> second = await _repository.GetProductByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.That(second.Value.Name, Is.Not.EqualTo("Changed outside the repository"));
    }
}
