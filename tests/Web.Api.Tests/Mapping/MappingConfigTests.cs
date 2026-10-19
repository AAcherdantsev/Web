using Mapster;
using Web.Api.Configuration.Mapping;
using Web.Api.Models;
using Web.Application.Common;
using Web.Domain.Entities;

namespace Web.Api.Tests.Mapping;

[TestFixture]
public class MappingConfigTests
{
    [SetUp]
    public void SetUp()
    {
        _config = new();
        new MappingConfig().Register(_config);
    }

    private TypeAdapterConfig _config;

    [Test]
    public void Product_MapsAllFieldsToProductDto()
    {
        // Arrange
        DateTimeOffset created = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        DateTimeOffset updated = new(2026, 2, 3, 4, 5, 6, TimeSpan.Zero);
        Product product = new()
        {
            Id = 7,
            Name = "Phone",
            ImgUri = "https://example.com/phone.jpg",
            Price = 99.5m,
            Description = "Description",
            CreatedAt = created,
            LastUpdatedAt = updated
        };

        // Act
        ProductDto dto = product.Adapt<ProductDto>(_config);

        // Assert
        Assert.That(dto.Id, Is.EqualTo(7));
        Assert.That(dto.Name, Is.EqualTo("Phone"));
        Assert.That(dto.ImgUri, Is.EqualTo("https://example.com/phone.jpg"));
        Assert.That(dto.Price, Is.EqualTo(99.5m));
        Assert.That(dto.Description, Is.EqualTo("Description"));
        Assert.That(dto.CreatedAt, Is.EqualTo(created));
        Assert.That(dto.LastUpdatedAt, Is.EqualTo(updated));
    }

    [Test]
    public void PagedList_MapsValuesAndPagingInformation()
    {
        // Arrange
        List<Product> products =
        [
            new() { Id = 1, Name = "A", ImgUri = "https://example.com/a.jpg", Price = 20 },
            new() { Id = 2, Name = "B", ImgUri = "https://example.com/b.jpg", Price = 10 }
        ];
        PagedList<Product> source = new(products, 5, 1, 2);

        // Act
        PagedList<ProductDto> result = source.Adapt<PagedList<ProductDto>>(_config);

        // Assert
        Assert.That(result.TotalCount, Is.EqualTo(5));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(2));
        Assert.That(result.Values.Select(v => v.Name), Is.EqualTo(["A", "B"]));
    }
}
