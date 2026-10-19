using System.Net;
using System.Text.Json;
using Moq;
using Web.Api.Tests.IntegrationTests;
using Web.Api.Tests.TestData;
using Web.Domain.Entities;

namespace Web.Api.Tests.Endpoints.V1;

[TestFixture]
public class GetAllProductsEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task GetProducts_ReturnsEveryProductAsJson()
    {
        // Arrange
        IReadOnlyList<Product> products = [TestProducts.Create(1, "A", 10), TestProducts.Create(2, "B", 20)];
        ServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);

        // Act
        HttpResponseMessage response = await Client.GetAsync("/v1/products");
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        JsonElement items = document.RootElement;
        Assert.That(items.GetArrayLength(), Is.EqualTo(2));
        Assert.That(items[0].GetProperty("id").GetInt32(), Is.EqualTo(1));
        Assert.That(items[0].GetProperty("name").GetString(), Is.EqualTo("A"));
        Assert.That(items[1].GetProperty("price").GetDecimal(), Is.EqualTo(20m));
        Assert.That(items[0].GetProperty("createdAt").GetDateTimeOffset(), Is.EqualTo(TestProducts.Created));
    }

    [Test]
    public async Task GetProducts_WhenThereAreNone_ReturnsEmptyArray()
    {
        ServiceMock.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        HttpResponseMessage response = await Client.GetAsync("/v1/products");

        Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("[]"));
    }
}
