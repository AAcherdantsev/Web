using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Web.Api.Tests.IntegrationTests;

/// <summary>
///     Runs the whole HTTP pipeline on the Mock data source. No service is replaced, so the endpoints, the application
///     service, and the in-memory store are tested together. The other tests replace the service with a mock.
/// </summary>
[TestFixture]
public class MockDataSourceTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void StartApi()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseSetting("DataSource", "Mock"));
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void StopApi()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task GetAllProducts_ReturnsTheSeedProducts()
    {
        HttpResponseMessage response = await _client.GetAsync("/v1/products");
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(document.RootElement.GetArrayLength(), Is.GreaterThan(0));
    }

    [Test]
    public async Task GetProductById_ReturnsTheProduct()
    {
        HttpResponseMessage response = await _client.GetAsync("/v1/products/1");
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(document.RootElement.GetProperty("id").GetInt32(), Is.EqualTo(1));
        Assert.That(document.RootElement.GetProperty("name").GetString(), Is.EqualTo("iPhone 18 Pro"));
    }

    [Test]
    public async Task GetProductById_WhenMissing_Returns404()
    {
        HttpResponseMessage response = await _client.GetAsync("/v1/products/999999");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PatchProduct_ThenGet_ReturnsTheNewDescription()
    {
        HttpResponseMessage patch = await SendPatchAsync("/v1/products/3", "{\"description\":\"Changed in test\"}");
        HttpResponseMessage get = await _client.GetAsync("/v1/products/3");
        using JsonDocument document = JsonDocument.Parse(await get.Content.ReadAsStringAsync());

        Assert.That(patch.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(document.RootElement.GetProperty("description").GetString(), Is.EqualTo("Changed in test"));
    }

    [Test]
    public async Task PatchProduct_WhenMissing_Returns404()
    {
        HttpResponseMessage response = await SendPatchAsync("/v1/products/999999", "{\"description\":\"x\"}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetPagedProducts_ReturnsTheSameTotalAsTheFullList()
    {
        HttpResponseMessage all = await _client.GetAsync("/v1/products");
        using JsonDocument allDocument = JsonDocument.Parse(await all.Content.ReadAsStringAsync());
        int expectedTotal = allDocument.RootElement.GetArrayLength();

        HttpResponseMessage response = await _client.GetAsync("/v2/products?pageNumber=1&pageSize=5");
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement root = document.RootElement;

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(root.GetProperty("totalCount").GetInt64(), Is.EqualTo(expectedTotal));
        Assert.That(root.GetProperty("values").GetArrayLength(), Is.EqualTo(Math.Min(5, expectedTotal - 5)));
        Assert.That(root.GetProperty("totalPages").GetInt64(), Is.EqualTo((expectedTotal + 4) / 5));
    }

    private Task<HttpResponseMessage> SendPatchAsync(string url, string json)
    {
        HttpRequestMessage request = new(HttpMethod.Patch, url)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
        return _client.SendAsync(request);
    }
}
