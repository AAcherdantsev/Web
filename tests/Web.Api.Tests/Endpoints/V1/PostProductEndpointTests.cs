using System.Net;
using System.Text.Json;
using FluentResults;
using Moq;
using Web.Api.Tests.IntegrationTests;
using Web.Api.Tests.TestData;
using Web.Application.Products;

namespace Web.Api.Tests.Endpoints.V1;

[TestFixture]
public class PostProductEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task PostProduct_Returns201WithLocationAndProduct()
    {
        // Arrange
        ServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(TestProducts.Create(7, "Phone", 10)));

        // Act
        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Post, "/v1/products",
            "{\"name\":\"Phone\",\"imgUri\":\"https://example.com/p.jpg\",\"price\":10}");
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.Location?.ToString(), Is.EqualTo("/v1/products/7"));
        Assert.That(document.RootElement.GetProperty("name").GetString(), Is.EqualTo("Phone"));
    }

    [Test]
    public async Task PostProduct_WithNullDescription_PassesAnEmptyDescriptionToTheService()
    {
        // Arrange: a JSON null is not a description, so it must not reach the business rules as a null
        CreateProductCommand? received = null;
        ServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CreateProductCommand, CancellationToken>((command, _) => received = command)
            .ReturnsAsync(Result.Ok(TestProducts.Create(7)));

        // Act
        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Post, "/v1/products",
            "{\"name\":\"Phone\",\"imgUri\":\"https://example.com/p.jpg\",\"price\":10,\"description\":null}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(received?.Description, Is.Null.Or.Empty);
    }

    [Test]
    public async Task PostProduct_WhenServiceRejectsTheProduct_Returns400ProblemDetails()
    {
        ServiceMock.Setup(s => s.CreateAsync(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<ProductModel>("Price must be greater than 0."));

        HttpResponseMessage response = await SendJsonAsync(HttpMethod.Post, "/v1/products",
            "{\"name\":\"Phone\",\"imgUri\":\"https://example.com/p.jpg\",\"price\":10}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
