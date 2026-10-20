using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Web.Application.Common;
using Web.Application.Common.Repositories;
using Web.Domain.Entities;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Web.Infrastructure.Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Web.Infrastructure.Persistence.Repositories;

/// <summary>
///     Stores products in SQL Server through Entity Framework Core. The domain entities are converted to and from
///     <see cref="ProductRecord" />, which is the only type that the database context knows.
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

        List<ProductRecord> records = await _context.Products
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return records.Select(r => r.ToDomain()).ToList();
    }

    /// <inheritdoc />
    public async Task<PagedList<Product>> GetPagedAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting a page of products...");

        IQueryable<ProductRecord> records = _context.Products
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .AsNoTracking();

        int totalCount = await records.CountAsync(cancellationToken);

        List<ProductRecord> values = await records
            .Skip(pageNumber * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new(values.Select(r => r.ToDomain()), totalCount, pageNumber, pageSize);
    }

    /// <inheritdoc />
    public async Task<VersionedProduct?> FindByIdAsync(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting product with ID {productId}...", id);

        // Not tracked: the version is passed back to UpdateAsync explicitly, so no change is saved behind the caller's back
        ProductRecord? record = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return record?.ToVersioned();
    }

    /// <inheritdoc />
    public async Task<Product> AddAsync(Product product, CancellationToken cancellationToken)
    {
        ProductRecord record = ProductRecord.FromDomain(product, []);

        _context.Products.Add(record);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product {ProductId} created successfully", record.Id);
        return record.ToDomain();
    }

    /// <inheritdoc />
    public async Task<Product> UpdateAsync(VersionedProduct product, CancellationToken cancellationToken)
    {
        // The record carries the version that was read, so the UPDATE matches no row when another change was saved since
        ProductRecord record = ProductRecord.FromDomain(product.Product, product.Version);

        // A record with the same key may still be tracked, for example one that AddAsync stored in this context.
        // It is detached, so that the record of this update can be attached.
        foreach (EntityEntry<ProductRecord> tracked in _context.ChangeTracker.Entries<ProductRecord>()
                     .Where(entry => entry.Entity.Id == record.Id)
                     .ToList())
        {
            tracked.State = EntityState.Detached;
        }

        _context.Products.Update(record);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Product with ID {ProductId} was changed by another request", record.Id);
            throw new ProductConflictException(record.Id);
        }

        _logger.LogInformation("Product {ProductId} updated successfully", record.Id);
        return record.ToDomain();
    }
}
