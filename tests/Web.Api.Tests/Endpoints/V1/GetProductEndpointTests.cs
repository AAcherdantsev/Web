using FluentResults;
using Moq;
using Web.Api.Tests.IntegrationTests;
using Web.Domain.Entities;

namespace Web.Api.Tests.Endpoints.V1;

[TestFixture]
public class GetProductEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task GetProduct_ReturnsProduct_WhenRepositoryIsSuccessful()
    {
        // Arrange
        int productId = 1;
        Product domainProduct = new()
        {
            Id = productId,
            Name = "Test Product",
            ImgUri = "http://image",
            Price = 10,
            Description = "desc",
            CreatedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        RepositoryMock.Setup(r => r.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(domainProduct));

        // Act
        HttpResponseMessage response = await Client.GetAsync($"/v1/products/{productId}");

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
        RepositoryMock.Verify(r => r.GetProductByIdAsync(productId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
