using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Web.Api.Tests.Operations;

/// <summary>
///     Verifies the behaviour that differs in the Production environment.
/// </summary>
[TestFixture]
public class ProductionConfigurationTests
{
    private static WebApplicationFactory<Program> CreateProductionFactory()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.UseSetting("ConnectionStrings:WebDb",
                "Server=127.0.0.1,1;Database=Unreachable;Trusted_Connection=True;Connect Timeout=1;TrustServerCertificate=True");
        });
    }

    [Test]
    public async Task Swagger_IsNotAvailable_InProduction()
    {
        // Arrange
        using WebApplicationFactory<Program> factory = CreateProductionFactory();
        using HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task ContentSecurityPolicy_IsSent_InProduction()
    {
        // Arrange
        using WebApplicationFactory<Program> factory = CreateProductionFactory();
        using HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health/live");

        // Assert
        Assert.That(response.Headers.Contains("Content-Security-Policy"), Is.True);
    }

    [Test]
    public async Task ReadinessProbe_ReturnsServiceUnavailable_WhenTheDatabaseIsUnreachable()
    {
        // Arrange
        using WebApplicationFactory<Program> factory = CreateProductionFactory();
        using HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health/ready");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
    }
}
