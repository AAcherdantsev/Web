using FluentResults;
using Web.Application.Common;
using Web.Application.Common.Errors;
using Web.Domain.Entities;

namespace Web.Application.Products;

/// <summary>
///     The use cases of the product catalogue. The HTTP layer calls these, and nothing else.
/// </summary>
public interface IProductService
{
    /// <summary>
    ///     Gets all products.
    /// </summary>
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets one page of products.
    /// </summary>
    Task<PagedList<Product>> GetPagedAsync(GetPagedProductsQuery query, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets one product. Fails with <see cref="NotFoundError" /> when the product does not exist.
    /// </summary>
    Task<Result<Product>> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a product. Fails when the command breaks a business rule.
    /// </summary>
    Task<Result<Product>> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Replaces the description of a product. Fails with <see cref="NotFoundError" /> when the product does not
    ///     exist, or when the description breaks a business rule.
    /// </summary>
    Task<Result<Product>> UpdateDescriptionAsync(int id, string? description, CancellationToken cancellationToken);
}
