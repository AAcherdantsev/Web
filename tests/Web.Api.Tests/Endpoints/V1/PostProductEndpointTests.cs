using System.Text;
using System.Text.Json;
using FluentResults;
using Moq;
using Web.Api.Features.Products.CreateProduct;
using Web.Api.Tests.IntegrationTests;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Api.Tests.Endpoints.V1;

[TestFixture]
public class PostProductEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task PostProduct_CreatesProduct_WhenRepositoryIsSuccessful()
    {
        // Arrange
        Product domainProduct = new()
        {
            Id = 1,
            Name = "Test Product",
            ImgUri = "http://image",
            Price = 10,
            Description = "desc",
            CreatedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        CreateProductRequest request = new()
        {
            Name = "Test Product",
            ImgUri = "http://image",
            Price = 10,
            Description = "desc"
        };

        string json = JsonSerializer.Serialize(request);
        StringContent content = new(json, Encoding.UTF8, "application/json");

        RepositoryMock.Setup(r => r.CreateProductAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(domainProduct));

        // Act
        HttpResponseMessage response = await Client.PostAsync("/v1/products", content);

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
        RepositoryMock.Verify(
            r => r.CreateProductAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task PostProduct_WithNullDescription_Returns201()
    {
        // Arrange: a JSON null is not an empty description and must not reach the business rules as a null
        string json = "{\"name\":\"Phone\",\"imgUri\":\"https://example.com/phone.jpg\",\"price\":10,\"description\":null}";
        StringContent content = new(json, Encoding.UTF8, "application/json");

        CreateProductCommand? received = null;
        RepositoryMock.Setup(r => r.CreateProductAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CreateProductCommand, CancellationToken>((command, _) => received = command)
            .ReturnsAsync(Result.Ok(new Product
            {
                Id = 1,
                Name = "Phone",
                ImgUri = "https://example.com/phone.jpg",
                Price = 10,
                CreatedAt = DateTimeOffset.UtcNow,
                LastUpdatedAt = DateTimeOffset.UtcNow
            }));

        // Act
        HttpResponseMessage response = await Client.PostAsync("/v1/products", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        Assert.That(received?.Description, Is.Null.Or.Empty);
    }
}
