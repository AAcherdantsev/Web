using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Web.Api.Tests.RateLimiting;

/// <summary>
///     Verifies that requests over the configured token bucket limits are rejected with 429.
/// </summary>
/// <remarks>
///     Each test uses its own host with small limits and a refill period of one hour,
///     so tokens do not come back during the test.
/// </remarks>
[TestFixture]
public class RateLimitingTests
{
    private static Dictionary<string, string?> LimitSettings(int readLimit = 2, int writeLimit = 1)
    {
        return new()
        {
            ["DataSource"] = "Mock",
            ["RateLimiting:Reads:TokenLimit"] = readLimit.ToString(),
            ["RateLimiting:Reads:TokensPerPeriod"] = readLimit.ToString(),
            ["RateLimiting:Reads:ReplenishmentPeriodSeconds"] = "3600",
            ["RateLimiting:Writes:TokenLimit"] = writeLimit.ToString(),
            ["RateLimiting:Writes:TokensPerPeriod"] = writeLimit.ToString(),
            ["RateLimiting:Writes:ReplenishmentPeriodSeconds"] = "3600"
        };
    }

    private static WebApplicationFactory<Program> CreateFactory(Dictionary<string, string?> settings)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            settings.Aggregate(builder, (host, setting) => host.UseSetting(setting.Key, setting.Value)));
    }

    private static HttpRequestMessage CreatePost()
    {
        return new(HttpMethod.Post, "/v1/products")
        {
            Content = new StringContent("{\"name\":\"Phone\",\"imgUri\":\"https://example.com/p.jpg\",\"price\":5}",
                Encoding.UTF8, "application/json")
        };
    }

    [Test]
    public async Task ReadRequests_BeyondTheLimit_Return429WithRetryAfterAndProblemDetails()
    {
        // Arrange
        using WebApplicationFactory<Program> factory = CreateFactory(LimitSettings(2));
        using HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage first = await client.GetAsync("/v1/products");
        HttpResponseMessage second = await client.GetAsync("/v1/products");
        HttpResponseMessage third = await client.GetAsync("/v1/products");

        // Assert
        Assert.That(first.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(third.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(third.Headers.RetryAfter, Is.Not.Null);
        Assert.That(third.Headers.RetryAfter!.Delta?.TotalSeconds, Is.GreaterThanOrEqualTo(1));
        Assert.That(third.Content.Headers.ContentType?.MediaType, Is.EqualTo("application/problem+json"));
    }

    [Test]
    public async Task WriteRequests_HaveTheirOwnLimit_SeparateFromReads()
    {
        // Arrange: exhaust the read limit first
        using WebApplicationFactory<Program> factory = CreateFactory(LimitSettings(1, 1));
        using HttpClient client = factory.CreateClient();
        await client.GetAsync("/v1/products");
        HttpResponseMessage readAfterLimit = await client.GetAsync("/v1/products");

        // Act
        HttpResponseMessage firstWrite = await client.SendAsync(CreatePost());
        HttpResponseMessage secondWrite = await client.SendAsync(CreatePost());

        // Assert
        Assert.That(readAfterLimit.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        Assert.That(firstWrite.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(secondWrite.StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
    }

    [Test]
    public void InvalidRateLimitSettings_StopTheApplicationAtStartup()
    {
        // Arrange: a bucket must hold at least one token
        Dictionary<string, string?> settings = LimitSettings();
        settings["RateLimiting:Reads:TokenLimit"] = "0";
        using WebApplicationFactory<Program> factory = CreateFactory(settings);

        // Act & Assert
        Assert.That(() => factory.CreateClient(), Throws.Exception);
    }
}
