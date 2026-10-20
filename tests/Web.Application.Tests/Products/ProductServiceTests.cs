using FluentResults;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Web.Application.Common;
using Web.Application.Common.Errors;
using Web.Application.Common.Repositories;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Application.Tests.Products;

/// <summary>
///     Tests the use cases of the product catalogue against a mocked store and a fake clock.
/// </summary>
[TestFixture]
public class ProductServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);

    private Mock<IProductRepository> _repository = null!;
    private FakeTimeProvider _timeProvider = null!;
    private ProductService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new();
        _timeProvider = new(Now);
        _service = new(_repository.Object, _timeProvider);

        // The store returns the product it was given, as the real stores do
        _repository.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);
        _repository.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);
    }

    [Test]
    public async Task CreateAsync_WithValidCommand_StoresProductWithCurrentTime()
    {
        CreateProductCommand command = new() { Name = "Phone", ImgUri = "https://example.com/p.jpg", Price = 10 };

        Result<ProductModel> result = await _service.CreateAsync(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.CreatedAt, Is.EqualTo(Now));
        _repository.Verify(r => r.AddAsync(
            It.Is<Product>(p => p.Name == "Phone" && p.CreatedAt == Now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WithInvalidCommand_FailsWithValidationErrorsAndStoresNothing()
    {
        CreateProductCommand command = new() { Name = "", ImgUri = "https://example.com/p.jpg", Price = 0 };

        Result<ProductModel> result = await _service.CreateAsync(command, CancellationToken.None);

        Assert.That(result.IsFailed, Is.True);
        Assert.That(result.Errors, Has.Count.EqualTo(2), "every broken rule is reported");
        Assert.That(result.Errors.All(e => e is ValidationError), Is.True);
        _repository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_WhenMissing_FailsWithNotFound()
    {
        _repository.Setup(r => r.FindByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync((Product?) null);

        Result<ProductModel> result = await _service.GetByIdAsync(7, CancellationToken.None);

        Assert.That(result.HasError<NotFoundError>(), Is.True);
    }

    [Test]
    public async Task UpdateDescriptionAsync_WhenValid_ChangesDescriptionAndStampsTheChange()
    {
        Product product = new("Phone", "https://example.com/p.jpg", 10, "old", Now.AddDays(-1));
        _repository.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        Result<ProductModel> result =
            await _service.UpdateDescriptionAsync(new() { Id = 1, Description = "new" }, CancellationToken.None);

        Assert.That(result.Value.Description, Is.EqualTo("new"));
        Assert.That(result.Value.LastUpdatedAt, Is.EqualTo(Now));
        _repository.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateDescriptionAsync_WithNullDescription_StoresEmptyDescription()
    {
        Product product = new("Phone", "https://example.com/p.jpg", 10, "old", Now);
        _repository.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        Result<ProductModel> result =
            await _service.UpdateDescriptionAsync(new() { Id = 1, Description = null }, CancellationToken.None);

        Assert.That(result.Value.Description, Is.Empty);
    }

    [Test]
    public async Task UpdateDescriptionAsync_TooLong_FailsBeforeReadingTheStore()
    {
        string tooLong = new('x', ProductLimits.MAX_DESCRIPTION_LENGTH + 1);

        Result<ProductModel> result =
            await _service.UpdateDescriptionAsync(new() { Id = 1, Description = tooLong }, CancellationToken.None);

        Assert.That(result.HasError<ValidationError>(), Is.True);
        _repository.Verify(r => r.FindByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UpdateDescriptionAsync_WhenMissing_FailsWithNotFoundAndStoresNothing()
    {
        _repository.Setup(r => r.FindByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync((Product?) null);

        Result<ProductModel> result =
            await _service.UpdateDescriptionAsync(new() { Id = 9, Description = "text" }, CancellationToken.None);

        Assert.That(result.HasError<NotFoundError>(), Is.True);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UpdateDescriptionAsync_WhenTheStoreReportsAConflict_FailsWithConflict()
    {
        Product product = new("Phone", "https://example.com/p.jpg", 10, "old", Now);
        _repository.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _repository.Setup(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ProductConflictException(1));

        Result<ProductModel> result =
            await _service.UpdateDescriptionAsync(new() { Id = 1, Description = "new" }, CancellationToken.None);

        Assert.That(result.HasError<ConflictError>(), Is.True);
    }

    [Test]
    public async Task GetPagedAsync_PassesThePageToTheStore()
    {
        _repository.Setup(r => r.GetPagedAsync(2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Product>([], 0, 2, 5));

        Result<PagedList<ProductModel>> result = await _service.GetPagedAsync(
            new() { PageNumber = 2, PageSize = 5 }, CancellationToken.None);

        Assert.That(result.Value.PageNumber, Is.EqualTo(2));
        _repository.Verify(r => r.GetPagedAsync(2, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestCase(-1, 10)]
    [TestCase(0, 0)]
    [TestCase(0, -5)]
    [TestCase(int.MaxValue, 2)]
    public async Task GetPagedAsync_OutOfRange_FailsBeforeReadingTheStore(int pageNumber, int pageSize)
    {
        Result<PagedList<ProductModel>> result = await _service.GetPagedAsync(
            new() { PageNumber = pageNumber, PageSize = pageSize }, CancellationToken.None);

        Assert.That(result.HasError<ValidationError>(), Is.True);
        _repository.Verify(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
