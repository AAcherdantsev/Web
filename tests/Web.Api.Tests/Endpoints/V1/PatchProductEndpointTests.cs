using System.Net;
using System.Text;
using System.Text.Json;
using FluentResults;
using Moq;
using Web.Api.Tests.IntegrationTests;
using Web.Application.Common.Errors;
using Web.Domain.Entities;

namespace Web.Api.Tests.Endpoints.V1;

[TestFixture]
public class PatchProductEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task PatchProduct_UpdatesDescription_WhenRepositoryIsSuccessful()
    {
        // Arrange
        const int productId = 1;
        const string newDescription = "Updated description";

        Product domainProduct = new()
        {
            Id = productId,
            Name = "Test Product",
            ImgUri = "http://image",
            Price = 10,
            Description = newDescription,
            CreatedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

        string json = JsonSerializer.Serialize(new { Description = newDescription });
        StringContent content = new(json, Encoding.UTF8, "application/json");

        RepositoryMock
            .Setup(r => r.UpdateProductDescriptionAsync(productId, newDescription, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(domainProduct));

        // Act
        HttpResponseMessage response = await Client.PatchAsync($"/v1/products/{productId}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        RepositoryMock.Verify(
            r => r.UpdateProductDescriptionAsync(productId, newDescription, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task PatchProduct_ReturnsNotFound_WhenProductDoesNotExist()
    {
        // Arrange
        RepositoryMock
            .Setup(r => r.UpdateProductDescriptionAsync(999, "text", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Product>(new NotFoundError("Product not found.")));

        string json = JsonSerializer.Serialize(new { Description = "text" });
        StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await Client.PatchAsync("/v1/products/999", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PatchProduct_ReturnsBadRequest_WhenDescriptionIsMissing()
    {
        // Arrange
        StringContent content = new("{}", Encoding.UTF8, "application/json");

        // Act
        HttpResponseMessage response = await Client.PatchAsync("/v1/products/1", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        RepositoryMock.Verify(
            r => r.UpdateProductDescriptionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
