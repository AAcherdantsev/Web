using Mapster;
using Web.Api.Configuration.Mapping;
using Web.Api.Models;
using Web.Api.Tests.TestData;
using Web.Application.Common;
using Web.Application.Products;

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

    private TypeAdapterConfig _config = null!;

    [Test]
    public void ProductModel_MapsAllFieldsToProductDto()
    {
        // Arrange
        DateTimeOffset updated = new(2026, 2, 3, 4, 5, 6, TimeSpan.Zero);
        ProductModel model = new()
        {
            Id = 7,
            Name = "Phone",
            ImgUri = "https://example.com/p.jpg",
            Price = 10,
            Description = "desc",
            CreatedAt = TestProducts.Created,
            LastUpdatedAt = updated
        };

        // Act
        ProductDto dto = model.Adapt<ProductDto>(_config);

        // Assert
        Assert.That(dto.Id, Is.EqualTo(7));
        Assert.That(dto.Name, Is.EqualTo("Phone"));
        Assert.That(dto.ImgUri, Is.EqualTo("https://example.com/p.jpg"));
        Assert.That(dto.Price, Is.EqualTo(10));
        Assert.That(dto.Description, Is.EqualTo("desc"));
        Assert.That(dto.CreatedAt, Is.EqualTo(TestProducts.Created));
        Assert.That(dto.LastUpdatedAt, Is.EqualTo(updated));
    }

    [Test]
    public void PagedList_MapsValuesAndPagingInformation()
    {
        // Arrange
        List<ProductModel> products =
        [
            TestProducts.Create(1, "A", 20),
            TestProducts.Create(2, "B", 10)
        ];
        PagedList<ProductModel> source = new(products, 5, 1, 2);

        // Act
        PagedList<ProductDto> result = source.Adapt<PagedList<ProductDto>>(_config);

        // Assert
        Assert.That(result.TotalCount, Is.EqualTo(5));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(2));
        Assert.That(result.TotalPages, Is.EqualTo(3));
        Assert.That(result.HasNext, Is.True);
        Assert.That(result.Values.Select(v => v.Name), Is.EqualTo(["A", "B"]));
    }
}
