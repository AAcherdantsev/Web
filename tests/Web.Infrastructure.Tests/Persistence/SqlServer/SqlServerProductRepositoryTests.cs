using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.MsSql;
using Web.Application.Common;
using Web.Application.Products;
using Web.Domain.Entities;
using Web.Infrastructure.Persistence;
using Web.Infrastructure.Persistence.Repositories;

namespace Web.Infrastructure.Tests.Persistence.SqlServer;

/// <summary>
///     Runs the product repository and the migrations against a real SQL Server container.
///     Needs a running Docker daemon.
/// </summary>
[TestFixture]
public class SqlServerProductRepositoryTests
{
    private MsSqlContainer _container = null!;
    private DatabaseContext _context = null!;
    private ProductRepository _repository = null!;

    [OneTimeSetUp]
    public async Task StartSqlServer()
    {
        _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await _container.StartAsync();

        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        _context = new(options);
        await _context.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task StopSqlServer()
    {
        await _context.DisposeAsync();
        await _container.DisposeAsync();
    }

    [SetUp]
    public async Task ResetData()
    {
        _context.ChangeTracker.Clear();
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Products");
        _repository = new(_context, new Mock<ILogger<ProductRepository>>().Object);
    }

    [Test]
    public async Task GetAllProductsAsync_PagesWithoutDuplicatesOrGaps_WhenCreatedAtIsEqual()
    {
        // Arrange: identical timestamps make the ordering depend on the ID tie-breaker
        DateTime createdAt = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (int i = 1; i <= 5; i++)
        {
            _context.Products.Add(new()
            {
                Name = $"Product {i}",
                ImgUri = "https://example.com/p.jpg",
                Price = i,
                CreatedAt = createdAt,
                LastUpdatedAt = createdAt
            });
        }

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        List<int> ids = [];
        for (int page = 0; page < 3; page++)
        {
            Result<PagedList<Product>> result = await _repository.GetAllProductsAsync(
                new() { PageNumber = page, PageSize = 2 }, CancellationToken.None);
            ids.AddRange(result.Value.Values.Select(p => p.Id));
        }

        // Assert
        Assert.That(ids, Has.Count.EqualTo(5));
        Assert.That(ids.Distinct().Count(), Is.EqualTo(5), "a product appeared on two pages");
        Assert.That(ids, Is.Ordered);
    }

    [Test]
    public async Task CreateProductAsync_PersistsTheProduct()
    {
        // Arrange
        CreateProductCommand command = new()
        {
            Name = "Real database",
            ImgUri = "https://example.com/db.jpg",
            Price = 42.5m,
            Description = "Stored in SQL Server"
        };

        // Act
        Result<Product> result = await _repository.CreateProductAsync(command, CancellationToken.None);
        _context.ChangeTracker.Clear();
        Product stored = await _context.Products.AsNoTracking().SingleAsync(p => p.Id == result.Value.Id);

        // Assert
        Assert.That(stored.Name, Is.EqualTo("Real database"));
        Assert.That(stored.Price, Is.EqualTo(42.5m));
    }

    [Test]
    public async Task UpdateProductDescriptionAsync_PersistsTheChange()
    {
        // Arrange
        Result<Product> created = await _repository.CreateProductAsync(
            new() { Name = "Update me", ImgUri = "https://example.com/u.jpg", Price = 1 }, CancellationToken.None);
        _context.ChangeTracker.Clear();

        // Act
        await _repository.UpdateProductDescriptionAsync(created.Value.Id, "Changed", CancellationToken.None);
        _context.ChangeTracker.Clear();
        Product stored = await _context.Products.AsNoTracking().SingleAsync(p => p.Id == created.Value.Id);

        // Assert
        Assert.That(stored.Description, Is.EqualTo("Changed"));
    }

    [Test]
    public async Task Schema_LimitsTextColumnsToTheDocumentedLengths()
    {
        // Act
        int nameLength = await _context.Database
            .SqlQuery<int>(
                $"SELECT CAST(CHARACTER_MAXIMUM_LENGTH AS int) AS [Value] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'Name'")
            .SingleAsync();
        int imgUriLength = await _context.Database
            .SqlQuery<int>(
                $"SELECT CAST(CHARACTER_MAXIMUM_LENGTH AS int) AS [Value] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'ImgUri'")
            .SingleAsync();

        // Assert
        Assert.That(nameLength, Is.EqualTo(ProductLimits.MAX_NAME_LENGTH));
        Assert.That(imgUriLength, Is.EqualTo(ProductLimits.MAX_IMG_URI_LENGTH));
    }

    [Test]
    public void Schema_RejectsANameLongerThanTheLimit()
    {
        // Act & Assert: the database enforces the limit even if a caller bypasses the validators
        string tooLong = new('x', ProductLimits.MAX_NAME_LENGTH + 1);
        Assert.That(
            async () => await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO Products (Name, ImgUri, Price, Description, CreatedAt, LastUpdatedAt) VALUES ({0}, 'https://example.com/x.jpg', 1, '', SYSUTCDATETIME(), SYSUTCDATETIME())",
                tooLong),
            Throws.Exception);
    }
}
