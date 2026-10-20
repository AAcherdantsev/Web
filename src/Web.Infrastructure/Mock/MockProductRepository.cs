using Microsoft.Extensions.Logging;
using Web.Application.Common;
using Web.Application.Common.Repositories;
using Web.Domain.Entities;
using Web.Infrastructure.Persistence;

namespace Web.Infrastructure.Mock;

/// <summary>
///     In-memory product store, pre-filled with sample products. Used by the Mock data source and by tests.
/// </summary>
/// <remarks>
///     The store keeps the same records as the database and behaves like it: every read returns a new domain product,
///     so changes are saved only through <see cref="UpdateAsync" />. An update fails with
///     <see cref="ProductConflictException" /> when the version is no longer the stored one.
/// </remarks>
internal class MockProductRepository : IProductRepository
{
    private readonly object _lock = new();
    private readonly ILogger<MockProductRepository> _logger;
    private readonly List<ProductRecord> _records = [];
    private int _lastId;
    private long _lastRowVersion;

    /// <summary>
    ///     Creates a new instance of the <see cref="MockProductRepository" /> class with the sample products.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="timeProvider">The source of the current time, used for the creation date of the sample products.</param>
    public MockProductRepository(ILogger<MockProductRepository> logger, TimeProvider timeProvider)
    {
        _logger = logger;

        foreach (Product sample in ProductSeedData.CreateProducts(timeProvider.GetUtcNow()))
        {
            _records.Add(Store(sample, ++_lastId));
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting all products from mock data...");
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            IReadOnlyList<Product> products = Ordered().Select(r => r.ToDomain()).ToList();
            return Task.FromResult(products);
        }
    }

    /// <inheritdoc />
    public Task<PagedList<Product>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting a page of products from mock data...");
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            List<ProductRecord> ordered = Ordered();
            // Same arithmetic as the database query. The application service keeps the offset inside the int range.
            List<Product> values = ordered.Skip(pageNumber * pageSize).Take(pageSize)
                .Select(r => r.ToDomain())
                .ToList();

            return Task.FromResult(new PagedList<Product>(values, ordered.Count, pageNumber, pageSize));
        }
    }

    /// <inheritdoc />
    public Task<VersionedProduct?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting product with ID {productId} from mock data...", id);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            ProductRecord? record = _records.FirstOrDefault(r => r.Id == id);
            return Task.FromResult(record?.ToVersioned());
        }
    }

    /// <inheritdoc />
    public Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            ProductRecord record = Store(product, ++_lastId);
            _records.Add(record);

            _logger.LogInformation("Product {ProductId} created successfully in mock data", record.Id);
            return Task.FromResult(record.ToDomain());
        }
    }

    /// <inheritdoc />
    public Task<Product> UpdateAsync(VersionedProduct product, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            int index = _records.FindIndex(r => r.Id == product.Product.Id);
            if (index < 0)
            {
                _logger.LogWarning("Product with ID {productId} not found. Unable to update the product.",
                    product.Product.Id);
                throw new ProductConflictException(product.Product.Id);
            }

            if (!_records[index].RowVersion.SequenceEqual(product.Version))
            {
                _logger.LogWarning("Product with ID {productId} was changed by another request.",
                    product.Product.Id);
                throw new ProductConflictException(product.Product.Id);
            }

            ProductRecord record = Store(product.Product, product.Product.Id);
            _records[index] = record;

            _logger.LogInformation("Product {ProductId} updated successfully in mock data", record.Id);
            return Task.FromResult(record.ToDomain());
        }
    }

    /// <summary>
    ///     Returns the stored records in the order of creation. Must be called while holding the lock.
    /// </summary>
    private List<ProductRecord> Ordered()
    {
        return _records.OrderBy(r => r.CreatedAt).ThenBy(r => r.Id).ToList();
    }

    /// <summary>
    ///     Creates the stored record of a product with the given identifier and a new row version. Must be called while
    ///     holding the lock.
    /// </summary>
    private ProductRecord Store(Product product, int id)
    {
        ProductRecord record = ProductRecord.FromDomain(product, BitConverter.GetBytes(++_lastRowVersion));
        record.Id = id;
        return record;
    }
}
