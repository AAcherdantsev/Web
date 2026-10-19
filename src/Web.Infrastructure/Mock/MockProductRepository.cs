using Microsoft.Extensions.Logging;
using Web.Application.Common;
using Web.Application.Common.Repositories;
using Web.Domain.Entities;

namespace Web.Infrastructure.Mock;

/// <summary>
///     In-memory product store, pre-filled with sample products. Used by the Mock data source and by tests.
/// </summary>
internal class MockProductRepository : IProductRepository
{
    private readonly object _lock = new();
    private readonly ILogger<MockProductRepository> _logger;
    private readonly List<Product> _products;
    private int _lastId;

    /// <summary>
    ///     Creates a new instance of the <see cref="MockProductRepository" /> class with the sample products.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="timeProvider">The source of the current time, used for the creation date of the sample products.</param>
    public MockProductRepository(ILogger<MockProductRepository> logger, TimeProvider timeProvider)
    {
        _logger = logger;
        _products = [];

        foreach (Product sample in ProductSeedData.CreateProducts(timeProvider.GetUtcNow()))
        {
            _products.Add(Clone(sample, ++_lastId));
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all products from mock data...");
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
        _logger.LogInformation("Getting a page of products from mock data...");
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            List<Product> ordered = Ordered();
            // Same arithmetic as the database query. The request validation keeps the offset inside the int range.
            List<Product> values = ordered.Skip(pageNumber * pageSize).Take(pageSize).Select(Copy).ToList();

            return Task.FromResult(new PagedList<Product>(values, ordered.Count, pageNumber, pageSize));
        }
    }

    /// <inheritdoc />
    public Task<Product?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product with ID {productId} from mock data...", id);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            Product? product = _products.FirstOrDefault(p => p.Id == id);
            return Task.FromResult(product is null ? null : Copy(product));
        }
    }

    /// <inheritdoc />
    public Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            Product stored = Clone(product, ++_lastId);
            _products.Add(stored);
            product.AssignId(stored.Id);

            _logger.LogInformation("Product {product} created successfully in mock data", product);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            int index = _products.FindIndex(p => p.Id == product.Id);
            if (index < 0)
            {
                _logger.LogWarning("Product with ID {productId} not found. Unable to update the product.", product.Id);
                return Task.CompletedTask;
            }

            _products[index] = Copy(product);

            _logger.LogInformation("Product {product} updated successfully in mock data", product);
            return Task.CompletedTask;
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
        return Clone(product, product.Id);
    }

    /// <summary>
    ///     Creates a copy of the product with the given ID. The ID is assigned by the repository, never by the caller.
    /// </summary>
    private static Product Clone(Product product, int id)
    {
        return Product.Restore(id, product.Name, product.ImgUri, product.Price, product.Description,
            product.CreatedAt, product.LastUpdatedAt);
    }
}
