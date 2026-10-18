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
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
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
}
