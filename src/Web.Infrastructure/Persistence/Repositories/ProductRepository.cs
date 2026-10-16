using System.Runtime.CompilerServices;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web.Application.Common;
using Web.Application.Common.Errors;
using Web.Application.Common.Repositories;
using Web.Application.Products;
using Web.Domain.Entities;

[assembly: InternalsVisibleTo("Web.Infrastructure.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Web.Infrastructure.Persistence.Repositories;

/// <summary>
///     Implementation of <see cref="IProductRepository" /> using Entity Framework Core.
/// </summary>
/// <remarks>
///     Database exceptions are intentionally not caught here. They propagate to the global exception handler,
///     so a failed query results in a 500 response instead of being reported as a client error.
/// </remarks>
internal class ProductRepository : IProductRepository
{
    private readonly DatabaseContext _context;
    private readonly ILogger<ProductRepository> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProductRepository" /> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public ProductRepository(DatabaseContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<Product>>> GetAllProductsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all products...");

        List<Product> products = await _context.Products
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return products;
    }

    /// <inheritdoc />
    public async Task<Result<PagedList<Product>>> GetAllProductsAsync(GetPagedProductsQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all products with pagination...");

        IQueryable<Product> products = _context.Products
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .AsNoTracking();

        int totalCount = await products.CountAsync(cancellationToken);

        List<Product> values = await products
            .Skip(query.PageNumber * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<Product>(values, totalCount, query.PageNumber, query.PageSize);
    }

    /// <inheritdoc />
    public async Task<Result<Product>> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product with ID {productId}...", productId);

        Product? product = await _context.Products.FindAsync([productId], cancellationToken);

        if (product != null)
        {
            return Result.Ok(product);
        }

        _logger.LogWarning("Product with ID {productId} not found", productId);
        return Result.Fail(new NotFoundError("Product not found."));
    }

    /// <inheritdoc />
    public async Task<Result<Product>> CreateProductAsync(CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        Result<Product> creation = ProductFactory.Create(command, DateTime.UtcNow);

        if (creation.IsFailed)
        {
            _logger.LogWarning("Invalid product data: {errors}. Unable to create the product.",
                string.Join("; ", creation.Errors.Select(e => e.Message)));
            return creation;
        }

        Product product = creation.Value;

        _logger.LogInformation("Creating product {product}...", product);

        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {product} created successfully", product);
        return Result.Ok(product);
    }

    /// <inheritdoc />
    public async Task<Result<Product>> UpdateProductDescriptionAsync(int productId, string description,
        CancellationToken cancellationToken)
    {
        Product? product = await _context.Products.FindAsync([productId], cancellationToken);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {productId} not found. Unable to update the product.", productId);
            return Result.Fail(new NotFoundError("Product not found."));
        }

        product.Description = description;
        product.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {product} updated successfully", product);
        return Result.Ok(product);
    }
}
