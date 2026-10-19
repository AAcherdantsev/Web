using FluentResults;
using Moq;
using Web.Api.Tests.IntegrationTests;
using Web.Application.Common;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Api.Tests.Endpoints.V2;

[TestFixture]
public class GetPagedProductListEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task GetPagedProducts_ReturnsPagedProducts_WhenRepositoryIsSuccessful()
    {
        // Arrange
        PagedList<Product> domainPagedList = new(
            new List<Product>
            {
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
            }, 1, 10, 1);

        RepositoryMock.Setup(r =>
                r.GetAllProductsAsync(It.IsAny<GetPagedProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(domainPagedList));

        // Act
        HttpResponseMessage response = await Client.GetAsync("/v2/products?pageNumber=1&pageSize=10");

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
        RepositoryMock.Verify(
            r => r.GetAllProductsAsync(It.IsAny<GetPagedProductsQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
