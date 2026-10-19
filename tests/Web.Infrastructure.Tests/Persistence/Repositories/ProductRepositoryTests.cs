using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Web.Application.Common;
using Web.Application.Common.Errors;
using Web.Application.Products;
using Web.Domain.Entities;
using Web.Infrastructure.Persistence;
using Web.Infrastructure.Persistence.Repositories;

namespace Web.Infrastructure.Tests.Persistence.Repositories;

/// <summary>
///     Unit tests for the <see cref="ProductRepository" /> class.
/// </summary>
[TestFixture]
public class ProductRepositoryTests
{
    [SetUp]
    public void SetUp()
    {
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new(options);
        _loggerMock = new();
        _repository = new(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    private DatabaseContext _context;
    private Mock<ILogger<ProductRepository>> _loggerMock;
    private ProductRepository _repository;

    [Test]
    public async Task GetAllProductsAsync_WithProducts_ReturnsAllProductsOrderedByCreatedAt()
    {
        // Arrange
        await _context.Products.AddRangeAsync(CreateTestProducts());
        await _context.SaveChangesAsync();

        // Act
        Result<IEnumerable<Product>> result = await _repository.GetAllProductsAsync(CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        List<Product> productList = result.Value.ToList();
        Assert.That(productList.Select(p => p.Name), Is.EqualTo(new[] { "Product 1", "Product 2", "Product 3" }));
    }

    [Test]
    public async Task GetAllProductsAsync_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Act
        Result<IEnumerable<Product>> result = await _repository.GetAllProductsAsync(CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.Empty);
    }

    [Test]
    public async Task GetAllProductsAsync_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        await _context.Products.AddRangeAsync(CreateTestProducts());
        await _context.SaveChangesAsync();

        GetPagedProductsQuery query = new() { PageNumber = 0, PageSize = 2 };

        // Act
        Result<PagedList<Product>> result = await _repository.GetAllProductsAsync(query, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.PageNumber, Is.EqualTo(0));
        Assert.That(result.Value.PageSize, Is.EqualTo(2));
        Assert.That(result.Value.TotalCount, Is.EqualTo(3));
        Assert.That(result.Value.Values.Select(p => p.Name), Is.EqualTo(new[] { "Product 1", "Product 2" }));
    }

    [Test]
    public async Task GetAllProductsAsync_WithPaginationSecondPage_ReturnsCorrectPage()
    {
        // Arrange
        await _context.Products.AddRangeAsync(CreateTestProducts());
        await _context.SaveChangesAsync();

        GetPagedProductsQuery query = new() { PageNumber = 1, PageSize = 2 };

        // Act
        Result<PagedList<Product>> result = await _repository.GetAllProductsAsync(query, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TotalCount, Is.EqualTo(3));
        Assert.That(result.Value.Values.Select(p => p.Name), Is.EqualTo(new[] { "Product 3" }));
    }

    [Test]
    public async Task GetAllProductsAsync_WithPaginationEmptyPage_ReturnsEmptyPage()
    {
        // Arrange
        await _context.Products.AddRangeAsync(CreateTestProducts());
        await _context.SaveChangesAsync();

        GetPagedProductsQuery query = new() { PageNumber = 5, PageSize = 2 };

        // Act
        Result<PagedList<Product>> result = await _repository.GetAllProductsAsync(query, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TotalCount, Is.EqualTo(3));
        Assert.That(result.Value.Values, Is.Empty);
    }

    [Test]
    public async Task GetProductByIdAsync_WithExistingProduct_ReturnsProduct()
    {
        // Arrange
        Product product = CreateTestProduct("Test Product", 99.99m);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        Result<Product> result = await _repository.GetProductByIdAsync(product.Id, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(product.Id));
        Assert.That(result.Value.Name, Is.EqualTo("Test Product"));
        Assert.That(result.Value.Price, Is.EqualTo(99.99m));
    }

    [Test]
    public async Task GetProductByIdAsync_WithNonExistentProduct_ReturnsNotFoundError()
    {
        // Act
        Result<Product> result = await _repository.GetProductByIdAsync(999, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Errors.First(), Is.TypeOf<NotFoundError>());
        Assert.That(result.Errors.First().Message, Is.EqualTo("Product not found."));
    }

    [Test]
    public async Task CreateProductAsync_WithValidCommand_CreatesAndReturnsProduct()
    {
        // Arrange
        CreateProductCommand command = new()
        {
            Name = "New Product",
            ImgUri = "https://example.com/image.jpg",
            Price = 49.99m,
            Description = "A great product"
        };

        // Act
        Result<Product> result = await _repository.CreateProductAsync(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo("New Product"));
        Assert.That(result.Value.ImgUri, Is.EqualTo("https://example.com/image.jpg"));
        Assert.That(result.Value.Price, Is.EqualTo(49.99m));
        Assert.That(result.Value.Description, Is.EqualTo("A great product"));
        Assert.That(result.Value.Id, Is.GreaterThan(0));

        Product? savedProduct = await _context.Products.FindAsync(result.Value.Id);
        Assert.That(savedProduct, Is.Not.Null);
        Assert.That(savedProduct.Name, Is.EqualTo("New Product"));
    }

    [Test]
    public async Task CreateProductAsync_WithoutDescription_CreatesProductWithEmptyDescription()
    {
        // Arrange
        CreateProductCommand command = new()
        {
            Name = "Minimal Product",
            ImgUri = "https://example.com/minimal.jpg",
            Price = 1
        };

        // Act
        Result<Product> result = await _repository.CreateProductAsync(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Description, Is.EqualTo(string.Empty));
    }

    [TestCase("", "https://example.com/image.jpg", 10, "Name cannot be empty.")]
    [TestCase("Product", "not-a-uri", 10, "Image URI is not valid.")]
    [TestCase("Product", "https://example.com/image.jpg", -1, "Price cannot be negative.")]
    public async Task CreateProductAsync_WithInvalidCommand_ReturnsFailureAndSavesNothing(
        string name, string imgUri, decimal price, string expectedMessage)
    {
        // Arrange
        CreateProductCommand command = new() { Name = name, ImgUri = imgUri, Price = price };

        // Act
        Result<Product> result = await _repository.CreateProductAsync(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Errors.First().Message, Is.EqualTo(expectedMessage));
        Assert.That(await _context.Products.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task CreateProductAsync_WithNameLongerThanTheLimit_ReturnsFailureAndSavesNothing()
    {
        // Arrange
        CreateProductCommand command = new()
        {
            Name = new('x', ProductLimits.MAX_NAME_LENGTH + 1),
            ImgUri = "https://example.com/image.jpg",
            Price = 1
        };

        // Act
        Result<Product> result = await _repository.CreateProductAsync(command, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
        Assert.That(await _context.Products.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async System.Threading.Tasks.Task CreateProductAsync_WhenDatabaseFails_ThrowsInsteadOfReturningFailure()
    {
        // Arrange
        CreateProductCommand command = new()
        {
            Name = "Test Product",
            ImgUri = "https://example.com/test.jpg",
            Price = 1
        };

        await _context.DisposeAsync();

        await

                // Act & Assert
                // Unexpected errors must reach the global exception handler (HTTP 500),
                // not be reported as a validation failure (HTTP 400).
                Assert.ThrowsAsync<ObjectDisposedException>(() =>
            _repository.CreateProductAsync(command, CancellationToken.None));
    }

    [Test]
    public async Task UpdateProductDescriptionAsync_WithExistingProduct_UpdatesDescriptionAndReturnsProduct()
    {
        // Arrange
        Product product = CreateTestProduct("Test Product", 99.99m);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        DateTimeOffset originalLastUpdatedAt = product.LastUpdatedAt;

        // Act
        Result<Product> result = await _repository.UpdateProductDescriptionAsync(product.Id, "New description",
            CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Description, Is.EqualTo("New description"));
        Assert.That(result.Value.LastUpdatedAt, Is.GreaterThan(originalLastUpdatedAt));

        Product? updatedProduct = await _context.Products.FindAsync(product.Id);
        Assert.That(updatedProduct, Is.Not.Null);
        Assert.That(updatedProduct!.Description, Is.EqualTo("New description"));
        Assert.That(updatedProduct.Name, Is.EqualTo("Test Product"));
    }

    [Test]
    public async Task UpdateProductDescriptionAsync_WithNonExistentProduct_ReturnsNotFoundError()
    {
        // Act
        Result<Product> result = await _repository.UpdateProductDescriptionAsync(999, "text", CancellationToken.None);

        // Assert
        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Errors.First(), Is.TypeOf<NotFoundError>());
        Assert.That(result.Errors.First().Message, Is.EqualTo("Product not found."));
    }

    [Test]
    public async Task UpdateProductDescriptionAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        Product product = CreateTestProduct("Test Product", 99.99m);
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        // Act & Assert
        Assert.That(async () => await _repository.UpdateProductDescriptionAsync(product.Id, "text", cts.Token),
            Throws.InstanceOf<OperationCanceledException>());
    }

    private static Product CreateTestProduct(string name, decimal price)
    {
        return new()
        {
            Name = name,
            ImgUri = "https://example.com/image.jpg",
            Price = price,
            Description = "Test description",
            CreatedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };
    }

    private static List<Product> CreateTestProducts()
    {
        DateTimeOffset baseTime = DateTimeOffset.UtcNow.AddHours(-3);

        return
        [
            new()
            {
                Name = "Product 1",
                ImgUri = "https://example.com/product1.jpg",
                Price = 10.99m,
                Description = "First product",
                CreatedAt = baseTime,
                LastUpdatedAt = baseTime
            },
            new()
            {
                Name = "Product 2",
                ImgUri = "https://example.com/product2.jpg",
                Price = 20.99m,
                Description = "Second product",
                CreatedAt = baseTime.AddMinutes(1),
                LastUpdatedAt = baseTime.AddMinutes(1)
            },
            new()
            {
                Name = "Product 3",
                ImgUri = "https://example.com/product3.jpg",
                Price = 30.99m,
                Description = "Third product",
                CreatedAt = baseTime.AddMinutes(2),
                LastUpdatedAt = baseTime.AddMinutes(2)
            }
        ];
    }
}
