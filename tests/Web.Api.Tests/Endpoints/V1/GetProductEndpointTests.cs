using System.Net;
using System.Text.Json;
using FluentResults;
using Moq;
using Web.Application.Common.Errors;
using Web.Api.Tests.IntegrationTests;
using Web.Api.Tests.TestData;
using Web.Application.Products;

namespace Web.Api.Tests.Endpoints.V1;

[TestFixture]
public class GetProductEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task GetProduct_ReturnsTheProductAsJson()
    {
        // Arrange
        ServiceMock.Setup(s => s.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(TestProducts.Create(3, "Phone", 99.5m, "A phone")));

        // Act
        HttpResponseMessage response = await Client.GetAsync("/v1/products/3");
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        JsonElement root = document.RootElement;
        Assert.That(root.GetProperty("id").GetInt32(), Is.EqualTo(3));
        Assert.That(root.GetProperty("name").GetString(), Is.EqualTo("Phone"));
        Assert.That(root.GetProperty("price").GetDecimal(), Is.EqualTo(99.5m));
        Assert.That(root.GetProperty("description").GetString(), Is.EqualTo("A phone"));
        Assert.That(root.GetProperty("imgUri").GetString(), Is.EqualTo("https://example.com/p.jpg"));
    }

    [Test]
    public async Task GetProduct_WhenMissing_Returns404()
    {
        ServiceMock.Setup(s => s.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<ProductModel>(new NotFoundError("Product not found.")));

        HttpResponseMessage response = await Client.GetAsync("/v1/products/3");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
