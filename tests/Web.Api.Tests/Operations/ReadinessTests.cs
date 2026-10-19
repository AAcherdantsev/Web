using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Web.Api.Tests.Operations;

/// <summary>
///     Checks that the readiness probe reports an unavailable dependency as <c>503 Service Unavailable</c>.
/// </summary>
[TestFixture]
public class ReadinessTests
{
    [Test]
    public async Task Ready_WhenADependencyIsUnhealthy_Returns503()
    {
        // Arrange: a stand-in for the database check that fails
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting("DataSource", "Mock");
                builder.ConfigureServices(services =>
                    services.AddHealthChecks().AddCheck("database",
                        () => HealthCheckResult.Unhealthy("The database does not accept connections.")));
            });
        using HttpClient client = factory.CreateClient();

        // Act
        HttpResponseMessage ready = await client.GetAsync("/health/ready");
        HttpResponseMessage live = await client.GetAsync("/health/live");

        // Assert: readiness reports the failure, liveness only confirms that the process responds
        Assert.That(ready.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
        Assert.That(live.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
