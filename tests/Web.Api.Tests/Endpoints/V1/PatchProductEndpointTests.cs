using System.Net;
using System.Text.Json;
using FluentResults;
using Moq;
using Web.Api.Tests.IntegrationTests;
using Web.Api.Tests.TestData;
using Web.Application.Common.Errors;
using Web.Application.Products;

namespace Web.Api.Tests.Endpoints.V1;

[TestFixture]
public class PatchProductEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task PatchProduct_ReturnsTheUpdatedProduct()
    {
        // Arrange
        ServiceMock.Setup(s =>
                s.UpdateDescriptionAsync(new() { Id = 4, Description = "New text" }, It.IsAny<CancellationToken>()))
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
        ServiceMock.Setup(s =>
                s.UpdateDescriptionAsync(new() { Id = 4, Description = "x" }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<ProductModel>(new NotFoundError("Product not found.")));

        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Patch, "/v1/products/4",
            "{\"description\":\"x\"}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PatchProduct_WhenAnotherChangeWasSavedFirst_Returns409WithProblemDetails()
    {
        ServiceMock.Setup(s =>
                s.UpdateDescriptionAsync(new() { Id = 4, Description = "x" }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<ProductModel>(
                new ConflictError("The product was changed by another request. Try again.")));

        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Patch, "/v1/products/4",
            "{\"description\":\"x\"}");
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
        Assert.That(document.RootElement.GetProperty("type").GetString(),
            Is.EqualTo("https://www.rfc-editor.org/rfc/rfc9110#section-15.5.10"));
        Assert.That(document.RootElement.GetProperty("detail").GetString(),
            Is.EqualTo("The product was changed by another request. Try again."));
    }
}
