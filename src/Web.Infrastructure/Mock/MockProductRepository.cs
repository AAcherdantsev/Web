using FluentResults;
using Microsoft.Extensions.Logging;
using Web.Application.Common;
using Web.Application.Common.Errors;
using Web.Application.Common.Repositories;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Infrastructure.Mock;

/// <summary>
///     In-memory implementation of <see cref="IProductRepository" />. Used instead of the database when the
///     <c>DataSource</c> setting is <c>Mock</c>. Data is lost when the application stops.
/// </summary>
internal class MockProductRepository : IProductRepository
{
    private readonly object _lock = new();
    private readonly ILogger<MockProductRepository> _logger;
    private readonly List<Product> _products;
    private int _lastId;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MockProductRepository" /> class, pre-filled with seed data.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public MockProductRepository(ILogger<MockProductRepository> logger)
    {
        _logger = logger;
        _products = ProductSeedData.CreateProducts(DateTimeOffset.UtcNow);

        for (int i = 0; i < _products.Count; i++)
        {
            _products[i] = Clone(_products[i], ++_lastId);
        }
    }

    /// <inheritdoc />
    public Task<Result<IEnumerable<Product>>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all products from mock data...");

        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            IEnumerable<Product> products = _products
                .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
                .Select(Copy)
                .ToList();

            return Task.FromResult(Result.Ok(products));
        }
    }

    /// <inheritdoc />
    public Task<Result<PagedList<Product>>> GetAllProductsAsync(GetPagedProductsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all products from mock data with pagination...");

        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            List<Product> ordered = _products.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToList();

            IEnumerable<Product> values = ordered.Skip(query.PageNumber * query.PageSize).Take(query.PageSize);

            PagedList<Product> pagedList = new(values.Select(Copy).ToList(), ordered.Count, query.PageNumber,
                query.PageSize);

            return Task.FromResult(Result.Ok(pagedList));
        }
    }

    /// <inheritdoc />
    public Task<Result<Product>> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product with ID {productId} from mock data...", productId);

        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            Product? product = _products.FirstOrDefault(p => p.Id == productId);

            if (product != null)
            {
                return Task.FromResult(Result.Ok(Copy(product)));
            }
        }

        _logger.LogWarning("Product with ID {productId} not found", productId);
        return Task.FromResult(Result.Fail<Product>(new NotFoundError("Product not found.")));
    }

    /// <inheritdoc />
    public Task<Result<Product>> CreateProductAsync(CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Result<Product> creation = ProductFactory.Create(command, DateTimeOffset.UtcNow);

        if (creation.IsFailed)
        {
            _logger.LogWarning("Invalid product data: {errors}. Unable to create the product.",
                string.Join("; ", creation.Errors.Select(e => e.Message)));
            return Task.FromResult(creation);
        }

        lock (_lock)
        {
            Product product = Clone(creation.Value, ++_lastId);
            _products.Add(product);

            _logger.LogInformation("Product {product} created successfully in mock data", product);
            return Task.FromResult(Result.Ok(Copy(product)));
        }
    }

    /// <inheritdoc />
    public Task<Result<Product>> UpdateProductDescriptionAsync(int productId, string description,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lock)
        {
            Product? product = _products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                _logger.LogWarning("Product with ID {productId} not found. Unable to update the product.", productId);
                return Task.FromResult(Result.Fail<Product>(new NotFoundError("Product not found.")));
            }

            product.Description = description;
            product.LastUpdatedAt = DateTimeOffset.UtcNow;

            _logger.LogInformation("Product {product} updated successfully in mock data", product);
            return Task.FromResult(Result.Ok(Copy(product)));
        }
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
        return new()
        {
            Id = id,
            Name = product.Name,
            ImgUri = product.ImgUri,
            Price = product.Price,
            Description = product.Description,
            CreatedAt = product.CreatedAt,
            LastUpdatedAt = product.LastUpdatedAt
        };
    }
}
