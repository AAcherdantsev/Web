using FluentResults;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Application.Common.Repositories;

/// <summary>
///     Repository interface for managing products.
/// </summary>
/// <remarks>
///     Expected outcomes (such as "not found" or invalid input) are returned as failed <see cref="Result" />s.
///     Unexpected failures (such as a lost database connection) are thrown as exceptions.
/// </remarks>
public interface IProductRepository
{
    /// <summary>
    ///     Retrieves all products.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<Result<IEnumerable<Product>>> GetAllProductsAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Retrieves a paginated list of products based on the query.
    /// </summary>
    /// <param name="query">The pagination query parameters.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<Result<PagedList<Product>>> GetAllProductsAsync(GetPagedProductsQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Retrieves a product by its ID.
    /// </summary>
    /// <param name="productId">The ID of the product.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<Result<Product>> GetProductByIdAsync(int productId, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a new product.
    /// </summary>
    /// <param name="command">The command containing product details.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<Result<Product>> CreateProductAsync(CreateProductCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Updates the description of an existing product.
    /// </summary>
    /// <param name="productId">The ID of the product to update.</param>
    /// <param name="description">The new description.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<Result<Product>> UpdateProductDescriptionAsync(int productId, string description,
        CancellationToken cancellationToken);
}
