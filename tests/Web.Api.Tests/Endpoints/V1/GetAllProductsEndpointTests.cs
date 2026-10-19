using FluentResults;
using Moq;
using Web.Api.Tests.IntegrationTests;
using Web.Domain.Entities;

namespace Web.Api.Tests.Endpoints.V1;

[TestFixture]
public class GetAllProductsEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task GetProducts_ReturnsProducts_WhenRepositoryIsSuccessful()
    {
        // Arrange
        List<Product> domainProducts =
        [
            new()
            {
                Id = 1,
                Name = "Test Product",
                ImgUri = "http://image",
                Price = 10,
                Description = "desc",
                CreatedAt = DateTimeOffset.UtcNow,
                LastUpdatedAt = DateTimeOffset.UtcNow
            }
        ];

        RepositoryMock.Setup(r => r.GetAllProductsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(domainProducts.AsEnumerable()));

        // Act
        HttpResponseMessage response = await Client.GetAsync("/v1/products");
        await response.Content.ReadAsStringAsync();

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
        RepositoryMock.Verify(r => r.GetAllProductsAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
