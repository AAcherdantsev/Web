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
    }

    [Test]
    public async Task CreateAsync_WithValidCommand_StoresProductWithCurrentTime()
    {
        CreateProductCommand command = new() { Name = "Phone", ImgUri = "https://example.com/p.jpg", Price = 10 };

        Result<Product> result = await _service.CreateAsync(command, CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        _repository.Verify(r => r.AddAsync(
            It.Is<Product>(p => p.Name == "Phone" && p.CreatedAt == Now),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WithInvalidCommand_FailsAndStoresNothing()
    {
        CreateProductCommand command = new() { Name = "Phone", ImgUri = "https://example.com/p.jpg", Price = 0 };

        Result<Product> result = await _service.CreateAsync(command, CancellationToken.None);

        Assert.That(result.IsFailed, Is.True);
        _repository.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetByIdAsync_WhenMissing_FailsWithNotFound()
    {
        _repository.Setup(r => r.FindByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync((Product?) null);

        Result<Product> result = await _service.GetByIdAsync(7, CancellationToken.None);

        Assert.That(result.HasError<NotFoundError>(), Is.True);
    }

    [Test]
    public async Task UpdateDescriptionAsync_WhenValid_ChangesDescriptionAndStampsTheChange()
    {
        Product product = new("Phone", "https://example.com/p.jpg", 10, "old", Now.AddDays(-1));
        _repository.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        Result<Product> result = await _service.UpdateDescriptionAsync(new () { Id = 1, Description = "new"}, CancellationToken.None);

        Assert.That(result.Value.Description, Is.EqualTo("new"));
        Assert.That(result.Value.LastUpdatedAt, Is.EqualTo(Now));
        _repository.Verify(r => r.UpdateAsync(product, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task UpdateDescriptionAsync_WithNullDescription_StoresEmptyDescription()
    {
        Product product = new("Phone", "https://example.com/p.jpg", 10, "old", Now);
        _repository.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        Result<Product> result = await _service.UpdateDescriptionAsync(new() { Id = 1, Description = null}, CancellationToken.None);

        Assert.That(result.Value.Description, Is.Empty);
    }

    [Test]
    public async Task UpdateDescriptionAsync_TooLong_FailsBeforeReadingTheStore()
    {
        string tooLong = new('x', ProductLimits.MAX_DESCRIPTION_LENGTH + 1);

        Result<Product> result = await _service.UpdateDescriptionAsync(new() { Id = 1, Description = tooLong}, CancellationToken.None);

        Assert.That(result.IsFailed, Is.True);
        _repository.Verify(r => r.FindByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task UpdateDescriptionAsync_WhenMissing_FailsWithNotFoundAndStoresNothing()
    {
        _repository.Setup(r => r.FindByIdAsync(9, It.IsAny<CancellationToken>())).ReturnsAsync((Product?) null);

        Result<Product> result = await _service.UpdateDescriptionAsync(new() { Id = 9, Description = "text"}, CancellationToken.None);

        Assert.That(result.HasError<NotFoundError>(), Is.True);
        _repository.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task GetPagedAsync_PassesThePageToTheStore()
    {
        _repository.Setup(r => r.GetPagedAsync(2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Product>([], 0, 2, 5));

        PagedList<Product> page = await _service.GetPagedAsync(
            new() { PageNumber = 2, PageSize = 5 }, CancellationToken.None);

        Assert.That(page.PageNumber, Is.EqualTo(2));
        _repository.Verify(r => r.GetPagedAsync(2, 5, It.IsAny<CancellationToken>()), Times.Once);
    }
}
