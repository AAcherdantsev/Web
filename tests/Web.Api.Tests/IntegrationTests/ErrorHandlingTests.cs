using System.Net;
using System.Net.Mime;
using System.Text.Json;
using FluentResults;
using Moq;
using Web.Api.Errors;
using Web.Api.Tests.TestData;
using Web.Application.Common.Errors;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Api.Tests.IntegrationTests;

/// <summary>
///     Verifies the REST contract: status codes, RFC 7807 error bodies, and response headers.
/// </summary>
[TestFixture]
public class ErrorHandlingTests : IntegrationTestBase
{
    [Test]
    public async Task GetProduct_WhenTheServiceThrows_Returns500ProblemDetails()
    {
        // Arrange
        ServiceMock.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database is unavailable"));

        // Act
        HttpResponseMessage response = await Client.GetAsync("/v1/products/1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo(MediaTypeNames.Application.ProblemJson));
        Assert.That(await response.Content.ReadAsStringAsync(), Does.Contain("An unexpected error occurred."));
    }

    [Test]
    public async Task PostProduct_WithInvalidFields_Returns400ProblemDetailsWithEveryError()
    {
        // Arrange
        string json = JsonSerializer.Serialize(new { Name = "", ImgUri = "not-a-url", Price = 0 });

        // Act
        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Post, "/v1/products", json);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo(MediaTypeNames.Application.ProblemJson));
        string body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("name").And.Contain("price").And.Contain("imgUri"));
        ServiceMock.Verify(s => s.CreateAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task PostProduct_WithMalformedJson_Returns400NotServerError()
    {
        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Post, "/v1/products", "{ this is not json");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetProduct_WhenMissing_Returns404ProblemDetailsWithTheSameMembersAsOtherErrors()
    {
        // Arrange
        ServiceMock.Setup(s => s.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Product>(new NotFoundError("Product not found.")));

        // Act
        HttpResponseMessage response = await Client.GetAsync("/v1/products/999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo(MediaTypeNames.Application.ProblemJson));

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement root = document.RootElement;
        Assert.That(root.GetProperty("type").GetString(), Is.EqualTo(ProblemResults.NOT_FOUND_TYPE));
        Assert.That(root.GetProperty("title").GetString(), Is.EqualTo("Resource not found."));
        Assert.That(root.GetProperty("detail").GetString(), Is.EqualTo("Product not found."));
        Assert.That(root.GetProperty("instance").GetString(), Is.EqualTo("/v1/products/999"));
        Assert.That(root.TryGetProperty("traceId", out _), Is.True);
    }

    [Test]
    public async Task PostProduct_WhenValidationFails_Returns400WithTheSameProblemTypeAsOtherErrors()
    {
        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Post, "/v1/products",
            "{\"name\":\"\",\"imgUri\":\"https://example.com/p.jpg\",\"price\":1}");

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(document.RootElement.GetProperty("type").GetString(), Is.EqualTo(ProblemResults.BAD_REQUEST_TYPE));
        Assert.That(document.RootElement.TryGetProperty("traceId", out _), Is.True);
    }

    [Test]
    public async Task PostProduct_WhenCreated_Returns201WithLocationHeader()
    {
        // Arrange
        ServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(TestProducts.Create(42, "Phone")));

        // Act
        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Post, "/v1/products",
            "{\"name\":\"Phone\",\"imgUri\":\"https://example.com/phone.jpg\",\"price\":10}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.Location?.ToString(), Is.EqualTo("/v1/products/42"));
    }

    [Test]
    public async Task PatchProduct_WhenMissing_Returns404ProblemDetails()
    {
        // Arrange
        ServiceMock.Setup(s => s.UpdateDescriptionAsync(999, "x", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Product>(new NotFoundError("Product not found.")));

        // Act
        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Patch, "/v1/products/999",
            "{\"description\":\"x\"}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo(MediaTypeNames.Application.ProblemJson));
    }

    [Test]
    public async Task GetPagedProducts_WithPageSizeAboveMaximum_Returns400()
    {
        HttpResponseMessage response = await Client.GetAsync("/v2/products?pageSize=100000");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo(MediaTypeNames.Application.ProblemJson));
        ServiceMock.Verify(s => s.GetPagedAsync(It.IsAny<GetPagedProductsQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetPagedProducts_WhenOffsetOverflowsInt_Returns400()
    {
        HttpResponseMessage response = await Client.GetAsync("/v2/products?pageNumber=2147483647&pageSize=100");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
