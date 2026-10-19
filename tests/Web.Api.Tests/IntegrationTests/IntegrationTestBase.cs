using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Web.Application.Common.Repositories;
using Web.Application.Products;

namespace Web.Api.Tests.IntegrationTests;

/// <summary>
///     Starts the API in memory, with the product service replaced by a mock, so that the HTTP layer is tested alone.
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly Mock<IProductService> ServiceMock;

    protected IntegrationTestBase()
    {
        ServiceMock = new();

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                // Settings must be host settings: minimal hosting reads builder.Configuration before ConfigureAppConfiguration runs
                builder.UseSetting("DataSource", "Mock");

                builder.ConfigureServices(services =>
                {
                    // Remove all hosted services to prevent background processing in tests
                    List<ServiceDescriptor> hostedServices =
                        services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
                    foreach (ServiceDescriptor service in hostedServices)
                    {
                        services.Remove(service);
                    }

                    services.RemoveAll<IProductRepository>();
                    services.RemoveAll<IProductService>();
                    services.AddSingleton(ServiceMock.Object);
                });
            });

        Client = Factory.CreateClient();
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }

    [SetUp]
    public void ResetMocks()
    {
        // NUnit reuses the fixture instance for all tests, so clear setups and recorded calls between tests
        ServiceMock.Reset();
    }

    /// <summary>
    ///     Sends a JSON body with the given method.
    /// </summary>
    protected Task<HttpResponseMessage> SendJsonAsync(HttpMethod method, string url, string json)
    {
        HttpRequestMessage request = new(method, url)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.Add(new("application/json"));
        return Client.SendAsync(request);
    }
}
