using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using FluentResults;
using Moq;
using Web.Application.Common;
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
    public async Task GetProduct_WhenRepositoryThrows_Returns500ProblemDetails()
    {
        // Arrange
        RepositoryMock.Setup(r => r.GetProductByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database is unavailable"));

        // Act
        HttpResponseMessage response = await Client.GetAsync("/v1/products/1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo(MediaTypeNames.Application.ProblemJson));
        string body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("An unexpected error occurred."));
    }

    [Test]
    public async Task PostProduct_WithInvalidFields_Returns400ProblemDetailsWithEveryError()
    {
        // Arrange
        string json = JsonSerializer.Serialize(new { Name = "", ImgUri = "not-a-url", Price = 0 });

        // Act
        HttpResponseMessage response = await PostJsonAsync("/v1/products", json);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo(MediaTypeNames.Application.ProblemJson));
        string body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("name").And.Contain("price").And.Contain("imgUri"));
        RepositoryMock.Verify(
            r => r.CreateProductAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task PostProduct_WithMalformedJson_Returns400NotServerError()
    {
        // Act
        HttpResponseMessage response = await PostJsonAsync("/v1/products", "{ this is not json");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PostProduct_WhenCreated_Returns201WithLocationHeader()
    {
        // Arrange
        Product created = new() { Id = 42, Name = "Phone", ImgUri = "https://example.com/phone.jpg", Price = 10 };
        RepositoryMock.Setup(r => r.CreateProductAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(created));

        string json =
            JsonSerializer.Serialize(new { Name = "Phone", ImgUri = "https://example.com/phone.jpg", Price = 10 });

        // Act
        HttpResponseMessage response = await PostJsonAsync("/v1/products", json);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.Location?.ToString(), Is.EqualTo("/v1/products/42"));
    }

    [Test]
    public async Task GetProduct_WhenMissing_Returns404ProblemDetails()
    {
        // Arrange
        RepositoryMock.Setup(r => r.GetProductByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Product>(new NotFoundError("Product not found.")));

        // Act
        HttpResponseMessage response = await Client.GetAsync("/v1/products/999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo(MediaTypeNames.Application.ProblemJson));

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.That(document.RootElement.GetProperty("title").GetString(), Is.EqualTo("Resource not found."));
        Assert.That(document.RootElement.GetProperty("detail").GetString(), Is.EqualTo("Product not found."));
    }

    [Test]
    public async Task PatchProduct_WhenMissing_Returns404ProblemDetails()
    {
        // Arrange
        RepositoryMock.Setup(r => r.UpdateProductDescriptionAsync(999, "x", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<Product>(new NotFoundError("Product not found.")));

        // Act
        HttpResponseMessage response =
            await PostJsonAsync("/v1/products/999", "{\"description\":\"x\"}", HttpMethod.Patch);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo(MediaTypeNames.Application.ProblemJson));
    }

    [Test]
    public async Task GetPagedProducts_WithoutPageSize_UsesConfiguredDefaultOf10()
    {
        // Arrange
        RepositoryMock
            .Setup(r => r.GetAllProductsAsync(It.IsAny<GetPagedProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new PagedList<Product>([], 0, 0, 10)));

        // Act
        HttpResponseMessage response = await Client.GetAsync("/v2/products");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        RepositoryMock.Verify(r => r.GetAllProductsAsync(
                It.Is<GetPagedProductsQuery>(q => q.PageSize == 10 && q.PageNumber == 0),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetPagedProducts_WithPageSizeAboveMaximum_Returns400()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync("/v2/products?pageSize=100000");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Content.Headers.ContentType?.MediaType,
            Is.EqualTo(MediaTypeNames.Application.ProblemJson));
        RepositoryMock.Verify(
            r => r.GetAllProductsAsync(It.IsAny<GetPagedProductsQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private Task<HttpResponseMessage> PostJsonAsync(string url, string json, HttpMethod? method = null)
    {
        HttpRequestMessage request = new(method ?? HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        request.Headers.Accept.Add(new(MediaTypeNames.Application.Json));
        return Client.SendAsync(request);
    }
}
