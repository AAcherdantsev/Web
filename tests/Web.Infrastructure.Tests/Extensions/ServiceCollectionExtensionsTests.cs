using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Web.Application.Common.Repositories;
using Web.Infrastructure.Extensions;
using Web.Infrastructure.Mock;
using Web.Infrastructure.Persistence;
using Web.Infrastructure.Persistence.Repositories;

namespace Web.Infrastructure.Tests.Extensions;

[TestFixture]
public class ServiceCollectionExtensionsTests
{
    private static IHostApplicationBuilder CreateBuilder(string? dataSource)
    {
        // The host defaults to Production, so these tests select Development explicitly
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = Environments.Development
        });
        builder.Configuration.AddInMemoryCollection([
            new("ConnectionStrings:WebDb", "Server=(localdb)\\MSSQLLocalDB;Database=TestDb;Trusted_Connection=True;"),
            new("DataSource", dataSource)
        ]);
        return builder;
    }

    [Test]
    public void AddRepository_DefaultDataSource_RegistersDatabaseContextAndRepository()
    {
        // Arrange
        IHostApplicationBuilder builder = CreateBuilder(null);

        // Act
        builder.AddRepository();
        ServiceProvider provider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.That(provider.GetRequiredService<DatabaseContext>(), Is.Not.Null);
        Assert.That(provider.GetRequiredService<IProductRepository>(), Is.InstanceOf<ProductRepository>());
    }

    [Test]
    public void AddRepository_MockDataSource_RegistersMockRepositoryWithoutDatabase()
    {
        // Arrange
        IHostApplicationBuilder builder = CreateBuilder("Mock");

        // Act
        builder.AddRepository();
        ServiceProvider provider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.That(provider.GetRequiredService<IProductRepository>(), Is.InstanceOf<MockProductRepository>());
        Assert.That(builder.Services.Any(d => d.ServiceType == typeof(DatabaseContext)), Is.False);
    }

    [Test]
    public void AddRepository_DataSourceIsCaseInsensitive()
    {
        // Arrange
        IHostApplicationBuilder builder = CreateBuilder("mock");

        // Act
        builder.AddRepository();
        ServiceProvider provider = builder.Services.BuildServiceProvider();

        // Assert
        Assert.That(provider.GetRequiredService<IProductRepository>(), Is.InstanceOf<MockProductRepository>());
    }

    [Test]
    public void AddRepository_UnknownDataSource_ThrowsWithClearMessage()
    {
        // Arrange
        IHostApplicationBuilder builder = CreateBuilder("Cloud");

        // Act & Assert
        InvalidOperationException? ex = Assert.Throws<InvalidOperationException>(() => builder.AddRepository());
        Assert.That(ex!.Message, Does.Contain("Cloud"));
    }

    [Test]
    public void AddRepository_Database_ConfiguresDbContextWithSqlServer()
    {
        // Arrange
        IHostApplicationBuilder builder = CreateBuilder("Database");

        // Act
        builder.AddRepository();
        ServiceProvider provider = builder.Services.BuildServiceProvider();

        // Assert: the provider name is public API, unlike the internal SqlServerOptionsExtension type
        DatabaseContext context = provider.GetRequiredService<DatabaseContext>();
        Assert.That(context.Database.ProviderName, Is.EqualTo("Microsoft.EntityFrameworkCore.SqlServer"));
    }

    [Test]
    public void AddRepository_MockDataSource_InProduction_ThrowsBeforeAnyRequestIsServed()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = Environments.Production
        });
        builder.Configuration.AddInMemoryCollection([new("DataSource", "Mock")]);

        Assert.That(builder.AddRepository, Throws.InstanceOf<InvalidOperationException>()
            .With.Message.Contains("Production"));
    }

    [Test]
    public void AddRepository_Database_WithoutConnectionString_ThrowsWithClearMessage()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new[]
        {
            new KeyValuePair<string, string?>("DataSource", "Database")
        });

        Assert.That(builder.AddRepository, Throws.InstanceOf<InvalidOperationException>()
            .With.Message.Contains("WebDb"));
    }
}
