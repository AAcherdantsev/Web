using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Web.Application.Common.Repositories;

namespace Web.Api.Tests.IntegrationTests;

public abstract class IntegrationTestBase : IDisposable
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly Mock<IProductRepository> RepositoryMock;

    public IntegrationTestBase()
    {
        RepositoryMock = new();

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

                    // Remove the existing services
                    services.RemoveAll(typeof(IProductRepository));

                    // Add mocked services
                    services.AddSingleton(RepositoryMock.Object);
                });
            });

        Client = Factory.CreateClient();
    }

    public void Dispose()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }

    [SetUp]
    public void ResetMocks()
    {
        // NUnit reuses the fixture instance for all tests, so clear setups and recorded calls between tests
        RepositoryMock.Reset();
    }
}
