using System.Net;
using System.Text.Json;
using FluentResults;
using Moq;
using Web.Api.Tests.IntegrationTests;
using Web.Api.Tests.TestData;
using Web.Application.Common.Errors;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Api.Tests.Endpoints.V1;

[TestFixture]
public class PatchProductEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task PatchProduct_ReturnsTheUpdatedProduct()
    {
        // Arrange
        ServiceMock.Setup(s => s.UpdateDescriptionAsync(new() { Id = 4, Description  = "New text" }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(TestProducts.Create(4, "Phone", 10, "New text")));

        // Act
        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Patch, "/v1/products/4",
            "{\"description\":\"New text\"}");
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(document.RootElement.GetProperty("description").GetString(), Is.EqualTo("New text"));
    }

    [Test]
    public async Task PatchProduct_WithNullDescription_Returns400AndDoesNotCallTheService()
    {
        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Patch, "/v1/products/4",
            "{\"description\":null}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        ServiceMock.Verify(s => s.UpdateDescriptionAsync(It.IsAny<UpdateProductDescriptionCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task PatchProduct_WhenMissing_Returns404()
    {
        ServiceMock.Setup(s => s.UpdateDescriptionAsync(new() { Id = 4, Description = "x"}, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Product>(new NotFoundError("Product not found.")));

        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Patch, "/v1/products/4",
            "{\"description\":\"x\"}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
