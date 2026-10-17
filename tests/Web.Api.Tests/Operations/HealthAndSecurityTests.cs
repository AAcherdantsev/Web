using System.Net;
using Web.Api.Tests.IntegrationTests;

namespace Web.Api.Tests.Operations;

/// <summary>
///     Verifies the operational endpoints and the security headers in the Development environment.
/// </summary>
[TestFixture]
public class HealthAndSecurityTests : IntegrationTestBase
{
    [Test]
    public async Task LivenessProbe_ReturnsOk()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync("/health/live");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task ReadinessProbe_ReturnsOk_WhenTheMockDataSourceIsUsed()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync("/health/ready");

        // Assert: the mock data source has no database to check
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Responses_CarrySecurityHeaders()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync("/v1/products");

        // Assert
        Assert.That(response.Headers.GetValues("X-Content-Type-Options").ToArray(), Is.EqualTo(["nosniff"]));
        Assert.That(response.Headers.GetValues("X-Frame-Options").ToArray(), Is.EqualTo(["DENY"]));
        Assert.That(response.Headers.Contains("Referrer-Policy"), Is.True);
    }

    [Test]
    public async Task SwaggerDocument_IsAvailable_InDevelopment()
    {
        // Act
        HttpResponseMessage response = await Client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
