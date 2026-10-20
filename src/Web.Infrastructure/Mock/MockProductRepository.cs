using Microsoft.Extensions.Logging;
using Web.Application.Common;
using Web.Application.Common.Repositories;
using Web.Domain.Entities;

namespace Web.Infrastructure.Mock;

/// <summary>
///     In-memory product store, pre-filled with sample products. Used by the Mock data source and by tests.
/// </summary>
/// <remarks>
///     The store behaves like the database store: every read returns a copy, so changes are saved only through
///     <see cref="UpdateAsync" />, and an update fails with <see cref="ProductConflictException" /> when the row version
///     is no longer the stored one.
/// </remarks>
internal class MockProductRepository : IProductRepository
{
    private readonly object _lock = new();
    private readonly ILogger<MockProductRepository> _logger;
    private readonly List<Product> _products = [];
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
            _products.Add(Store(sample, ++_lastId));
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting all products from mock data...");
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            IReadOnlyList<Product> products = Ordered().Select(Copy).ToList();
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
            List<Product> ordered = Ordered();
            // Same arithmetic as the database query. The application service keeps the offset inside the int range.
            List<Product> values = ordered.Skip(pageNumber * pageSize).Take(pageSize).Select(Copy).ToList();

            return Task.FromResult(new PagedList<Product>(values, ordered.Count, pageNumber, pageSize));
        }
    }

    /// <inheritdoc />
    public Task<Product?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Getting product with ID {productId} from mock data...", id);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            Product? product = _products.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(product is null ? null : Copy(product));
        }
    }

    /// <inheritdoc />
    public Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            Product stored = Store(product, ++_lastId);
            _products.Add(stored);

            _logger.LogInformation("Product {product} created successfully in mock data", stored);
            return Task.FromResult(Copy(stored));
        }
    }

    /// <inheritdoc />
    public Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            int index = _products.FindIndex(p => p.Id == product.Id);
            if (index < 0)
            {
                _logger.LogWarning("Product with ID {productId} not found. Unable to update the product.", product.Id);
                throw new ProductConflictException(product.Id);
            }

            if (!_products[index].RowVersion.SequenceEqual(product.RowVersion))
            {
                _logger.LogWarning("Product with ID {productId} was changed by another request.", product.Id);
                throw new ProductConflictException(product.Id);
            }

            Product stored = Store(product, product.Id);
            _products[index] = stored;

            _logger.LogInformation("Product {product} updated successfully in mock data", stored);
            return Task.FromResult(Copy(stored));
        }
    }

    /// <summary>
    ///     Returns the stored products in the order of creation. Must be called while holding the lock.
    /// </summary>
    private List<Product> Ordered()
    {
        return _products.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToList();
    }

    /// <summary>
    ///     Returns a copy of the product, so callers cannot change stored data without going through the repository.
    /// </summary>
    private static Product Copy(Product product)
    {
        return Product.Restore(product.Id, product.Name, product.ImgUri, product.Price, product.Description,
            product.CreatedAt, product.LastUpdatedAt, product.RowVersion);
    }

    /// <summary>
    ///     Creates the stored copy of a product with the given identifier and a new row version. Must be called while
    ///     holding the lock.
    /// </summary>
    private Product Store(Product product, int id)
    {
        return Product.Restore(id, product.Name, product.ImgUri, product.Price, product.Description,
            product.CreatedAt, product.LastUpdatedAt, BitConverter.GetBytes(++_lastRowVersion));
    }
}
