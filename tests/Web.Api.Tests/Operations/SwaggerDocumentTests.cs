using System.Net;
using System.Text.Json;
using Web.Api.Tests.IntegrationTests;

namespace Web.Api.Tests.Operations;

/// <summary>
///     Checks the OpenAPI documents that the Swagger UI shows, one per API version.
/// </summary>
[TestFixture]
public class SwaggerDocumentTests : IntegrationTestBase
{
    [Test]
    public async Task V1Document_DescribesTheVersionOneEndpoints()
    {
        using JsonDocument document = await GetDocumentAsync("/swagger/v1/swagger.json");

        JsonElement paths = document.RootElement.GetProperty("paths");
        Assert.That(paths.EnumerateObject().Select(p => p.Name),
            Is.EquivalentTo(new[] { "/v1/products", "/v1/products/{id}" }));
    }

    [Test]
    public async Task V2Document_DescribesThePagedListWithOptionalParameters()
    {
        using JsonDocument document = await GetDocumentAsync("/swagger/v2/swagger.json");

        JsonElement operation = document.RootElement.GetProperty("paths").GetProperty("/v2/products")
            .GetProperty("get");
        JsonElement parameters = operation.GetProperty("parameters");
        Assert.That(parameters.EnumerateArray().Select(p => p.GetProperty("name").GetString()),
            Is.EquivalentTo(new[] { "pageNumber", "pageSize" }));
        Assert.That(parameters.EnumerateArray().All(p => !p.TryGetProperty("required", out JsonElement required)
                                                         || !required.GetBoolean()),
            Is.True, "pageNumber and pageSize must be optional");
    }

    [Test]
    public async Task ProductSchema_MarksIdAndPriceAsRequired()
    {
        using JsonDocument document = await GetDocumentAsync("/swagger/v1/swagger.json");

        JsonElement product = document.RootElement.GetProperty("components").GetProperty("schemas")
            .GetProperty("ProductDto");
        List<string?> required = product.GetProperty("required").EnumerateArray()
            .Select(r => r.GetString()).ToList();
        Assert.That(required, Does.Contain("id").And.Contain("price"));
    }

    [Test]
    public async Task SwaggerUi_IsServedInDevelopment()
    {
        HttpResponseMessage response = await Client.GetAsync("/swagger/index.html");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    private async Task<JsonDocument> GetDocumentAsync(string url)
    {
        HttpResponseMessage response = await Client.GetAsync(url);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }
}
