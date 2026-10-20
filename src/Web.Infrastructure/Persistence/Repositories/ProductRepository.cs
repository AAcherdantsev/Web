using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Web.Application.Common;
using Web.Application.Common.Repositories;
using Web.Domain.Entities;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Web.Infrastructure.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Web.Infrastructure.Persistence.Repositories;

/// <summary>
///     Stores products in SQL Server through Entity Framework Core.
/// </summary>
internal class ProductRepository : IProductRepository
{
    private readonly DatabaseContext _context;
    private readonly ILogger<ProductRepository> _logger;

    /// <summary>
    ///     Creates a new instance of the <see cref="ProductRepository" /> class.
    /// </summary>
    public ProductRepository(DatabaseContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all products...");

        return await _context.Products
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PagedList<Product>> GetPagedAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting a page of products...");

        IQueryable<Product> products = _context.Products
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .AsNoTracking();

        int totalCount = await products.CountAsync(cancellationToken);

        List<Product> values = await products
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new(values, totalCount, pageNumber, pageSize);
    }

    /// <inheritdoc />
    public async Task<Product?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product with ID {productId}...", id);

        // Tracked, so that the changes made by the application service are saved by UpdateAsync
        return await _context.Products.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {product} created successfully", product);
        return product;
    }

    /// <inheritdoc />
    public async Task<Product> UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        if (_context.Entry(product).State == EntityState.Detached)
        {
            _context.Products.Update(product);
        }

        try
        {
            // The UPDATE checks the row version that was read, so a change saved in between makes it match no row
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Product with ID {ProductId} was changed by another request", product.Id);
            throw new ProductConflictException(product.Id);
        }

        _logger.LogInformation("Product {ProductId} updated successfully", product.Id);
        return product;
    }
}
