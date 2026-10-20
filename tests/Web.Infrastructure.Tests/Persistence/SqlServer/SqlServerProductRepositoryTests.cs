using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.MsSql;
using Web.Application.Common;
using Web.Application.Common.Repositories;
using Web.Domain.Entities;
using Web.Infrastructure.Persistence;
using Web.Infrastructure.Mock;
using Web.Infrastructure.Persistence.Repositories;
using Web.Infrastructure.Services;

namespace Web.Infrastructure.Tests.Persistence.SqlServer;

/// <summary>
///     Runs the product repository and the migrations against a real SQL Server container.
///     Needs a running Docker daemon, so the tests are in the <c>Docker</c> category.
/// </summary>
[TestFixture]
[Category("Docker")]
public class SqlServerProductRepositoryTests
{
    private static readonly DateTimeOffset Created = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private MsSqlContainer _container = null!;
    private DbContextOptions<DatabaseContext> _options = null!;
    private DatabaseContext _context = null!;
    private ProductRepository _repository = null!;

    [OneTimeSetUp]
    public async Task StartSqlServer()
    {
        _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").Build();
        await _container.StartAsync();

        _options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        _context = new(_options);
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
    public async Task GetPagedAsync_PagesWithoutDuplicatesOrGaps_WhenCreatedAtIsEqual()
    {
        // Arrange: identical timestamps make the ordering depend on the ID tie-breaker
        for (int i = 1; i <= 5; i++)
        {
            _context.Products.Add(new($"Product {i}", "https://example.com/p.jpg", i, "", Created));
        }

        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        // Act
        List<int> ids = [];
        for (int page = 0; page < 3; page++)
        {
            PagedList<Product> result = await _repository.GetPagedAsync(page, 2, CancellationToken.None);
            ids.AddRange(result.Values.Select(p => p.Id));
        }

        // Assert
        Assert.That(ids, Has.Count.EqualTo(5));
        Assert.That(ids.Distinct().Count(), Is.EqualTo(5), "a product appeared on two pages");
        Assert.That(ids, Is.Ordered);
    }

    [Test]
    public async Task AddAsync_PersistsTheProduct()
    {
        // Arrange
        Product product = new("Real database", "https://example.com/db.jpg", 42.5m, "Stored in SQL Server", Created);

        // Act
        await _repository.AddAsync(product, CancellationToken.None);
        _context.ChangeTracker.Clear();
        Product stored = await _context.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id);

        // Assert
        Assert.That(stored.Name, Is.EqualTo("Real database"));
        Assert.That(stored.Price, Is.EqualTo(42.5m));
    }

    [Test]
    public async Task UpdateAsync_PersistsTheChange()
    {
        // Arrange
        Product product = new("Update me", "https://example.com/u.jpg", 1, "", Created);
        await _repository.AddAsync(product, CancellationToken.None);
        _context.ChangeTracker.Clear();
        Product tracked = (await _repository.FindByIdAsync(product.Id, CancellationToken.None))!;

        // Act
        tracked.ChangeDescription("Changed", Created.AddDays(1));
        await _repository.UpdateAsync(tracked, CancellationToken.None);
        _context.ChangeTracker.Clear();
        Product stored = await _context.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id);

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
    public async Task Schema_StoresPriceWithTwoDecimalPlaces()
    {
        int scale = await _context.Database
            .SqlQuery<int>(
                $"SELECT CAST(NUMERIC_SCALE AS int) AS [Value] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'Price'")
            .SingleAsync();

        Assert.That(scale, Is.EqualTo(2));
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

    [Test]
    public async Task UpdateAsync_WhenAnotherChangeWasSavedFirst_ThrowsConflictAndKeepsTheFirstChange()
    {
        // Arrange: two requests read the same product, each with its own context
        Product product = new("Shared", "https://example.com/s.jpg", 1, "", Created);
        await _repository.AddAsync(product, CancellationToken.None);
        _context.ChangeTracker.Clear();

        await using DatabaseContext firstContext = new(_options);
        await using DatabaseContext secondContext = new(_options);
        ProductRepository first = new(firstContext, new Mock<ILogger<ProductRepository>>().Object);
        ProductRepository second = new(secondContext, new Mock<ILogger<ProductRepository>>().Object);

        Product firstCopy = (await first.FindByIdAsync(product.Id, CancellationToken.None))!;
        Product secondCopy = (await second.FindByIdAsync(product.Id, CancellationToken.None))!;
        byte[] readVersion = firstCopy.RowVersion;

        // Act: the first save succeeds and changes the row version, the second one must not overwrite it
        firstCopy.ChangeDescription("First", Created.AddDays(1));
        Product saved = await first.UpdateAsync(firstCopy, CancellationToken.None);
        secondCopy.ChangeDescription("Second", Created.AddDays(2));

        // Assert
        Assert.That(saved.RowVersion, Is.Not.EqualTo(readVersion));
        Assert.That(async () => await second.UpdateAsync(secondCopy, CancellationToken.None),
            Throws.TypeOf<ProductConflictException>());

        _context.ChangeTracker.Clear();
        Product stored = await _context.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id);
        Assert.That(stored.Description, Is.EqualTo("First"));
    }

    [Test]
    public async Task InitializeAsync_WhenTwoInstancesStartTogether_SeedsTheSampleProductsOnce()
    {
        // Arrange: an empty table, and two service providers that stand for two instances of the API
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Products");
        ServiceCollection services = new();
        services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(_container.GetConnectionString()));
        services.AddSingleton(TimeProvider.System);
        await using ServiceProvider first = services.BuildServiceProvider();
        await using ServiceProvider second = services.BuildServiceProvider();

        // Act
        await Task.WhenAll(
            DatabaseInitializer.InitializeAsync(first),
            DatabaseInitializer.InitializeAsync(second));

        // Assert
        int count = await _context.Products.AsNoTracking().CountAsync();
        Assert.That(count, Is.EqualTo(ProductSeedData.CreateProducts(DateTimeOffset.UtcNow).Count));
    }

    [Test]
    public async Task AddAsync_ReadsTimestampsWithUtcOffset()
    {
        // Arrange
        Product product = new("Offset", "https://example.com/o.jpg", 1, "", DateTimeOffset.UtcNow);
        DateTimeOffset before = DateTimeOffset.UtcNow;

        // Act
        await _repository.AddAsync(product, CancellationToken.None);
        _context.ChangeTracker.Clear();
        Product stored = await _context.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id);

        // Assert: a datetime2 column would read back without an offset, which would serialize without the offset
        Assert.That(stored.CreatedAt.Offset, Is.EqualTo(TimeSpan.Zero));
        Assert.That(stored.CreatedAt, Is.GreaterThanOrEqualTo(before.AddSeconds(-1)));
    }
}
