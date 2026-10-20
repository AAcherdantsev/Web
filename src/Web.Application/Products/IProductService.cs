using FluentResults;
using Web.Application.Common;
using Web.Application.Common.Errors;

namespace Web.Application.Products;

/// <summary>
///     The use cases of the product catalogue. The HTTP layer calls these, and nothing else.
/// </summary>
public interface IProductService
{
    /// <summary>
    ///     Gets all products.
    /// </summary>
    Task<IReadOnlyList<ProductModel>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    ///     Gets one page of products. Fails with <see cref="ValidationError" /> when the page is outside the supported
    ///     range.
    /// </summary>
    Task<Result<PagedList<ProductModel>>> GetPagedAsync(GetPagedProductsQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Gets one product. Fails with <see cref="NotFoundError" /> when the product does not exist.
    /// </summary>
    Task<Result<ProductModel>> GetByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    ///     Creates a product. Fails with <see cref="ValidationError" /> when the command breaks a business rule.
    /// </summary>
    Task<Result<ProductModel>> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Replaces the description of a product. Fails with <see cref="NotFoundError" /> when the product does not
    ///     exist, with <see cref="ValidationError" /> when the description breaks a business rule, and with
    ///     <see cref="ConflictError" /> when another change was saved while this one was being made.
    /// </summary>
    Task<Result<ProductModel>> UpdateDescriptionAsync(UpdateProductDescriptionCommand command,
        CancellationToken cancellationToken);
}
