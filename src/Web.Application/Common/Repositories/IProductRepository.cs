using Web.Domain.Entities;

namespace Web.Application.Common.Repositories;

/// <summary>
///     Stores and loads products. It does not check business rules: the entity and the application services do that.
/// </summary>
public interface IProductRepository
{
    /// <summary>
    ///     Gets all products in the order of creation.
    /// </summary>
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets one page of products in the order of creation.
    /// </summary>
    /// <param name="pageNumber">The zero-based page number.</param>
    /// <param name="pageSize">The number of products per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<PagedList<Product>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the product with the given identifier and the version it has now, or <see langword="null" /> when there
    ///     is none. The version must be passed back to <see cref="UpdateAsync" />.
    /// </summary>
    Task<VersionedProduct?> FindByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    ///     Stores a new product and returns the stored product with its identifier.
    /// </summary>
    Task<Product> AddAsync(Product product, CancellationToken cancellationToken);

    /// <summary>
    ///     Stores the changes of a product that was read at the version in <paramref name="product" />, and returns the
    ///     stored product.
    /// </summary>
    /// <exception cref="ProductConflictException">
    ///     Thrown when the product changed after it was read, or when it no longer exists.
    /// </exception>
    Task<Product> UpdateAsync(VersionedProduct product, CancellationToken cancellationToken);
}
