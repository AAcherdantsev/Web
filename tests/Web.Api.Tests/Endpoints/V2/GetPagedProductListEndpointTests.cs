using System.Net;
using System.Text.Json;
using Moq;
using Web.Api.Tests.IntegrationTests;
using Web.Api.Tests.TestData;
using Web.Application.Common;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Api.Tests.Endpoints.V2;

[TestFixture]
public class GetPagedProductListEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task GetPagedProducts_ReturnsPageInformationAndValues()
    {
        // Arrange
        ServiceMock.Setup(s => s.GetPagedAsync(It.IsAny<GetPagedProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Product>(
                [TestProducts.Create(3, "C"), TestProducts.Create(4, "D")], 7, 1, 2));

        // Act
        HttpResponseMessage response = await Client.GetAsync("/v2/products?pageNumber=1&pageSize=2");
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        JsonElement root = document.RootElement;
        Assert.That(root.GetProperty("pageNumber").GetInt32(), Is.EqualTo(1));
        Assert.That(root.GetProperty("pageSize").GetInt32(), Is.EqualTo(2));
        Assert.That(root.GetProperty("totalCount").GetInt64(), Is.EqualTo(7));
        Assert.That(root.GetProperty("values").GetArrayLength(), Is.EqualTo(2));
        Assert.That(root.GetProperty("values")[0].GetProperty("name").GetString(), Is.EqualTo("C"));
    }

    [Test]
    public async Task GetPagedProducts_WithoutPageNumber_RequestsTheFirstPage()
    {
        ServiceMock.Setup(s => s.GetPagedAsync(It.IsAny<GetPagedProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedList<Product>([], 0, 0, 10));

        HttpResponseMessage response = await Client.GetAsync("/v2/products");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        ServiceMock.Verify(s => s.GetPagedAsync(
            It.Is<GetPagedProductsQuery>(q => q.PageNumber == 0 && q.PageSize == 10), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
