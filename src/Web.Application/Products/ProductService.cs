using FluentResults;
using Web.Application.Common;
using Web.Application.Common.Errors;
using Web.Application.Common.Repositories;
using Web.Domain.Entities;

namespace Web.Application.Products;

/// <summary>
///     Implements the use cases of the product catalogue. Business rules and the current time are handled here,
///     and the repository only stores the products.
/// </summary>
public sealed class ProductService : IProductService
{
    private const string PRODUCT_NOT_FOUND = "Product not found.";

    private readonly IProductRepository _repository;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    ///     Creates a new instance of the <see cref="ProductService" /> class.
    /// </summary>
    /// <param name="repository">The product store.</param>
    /// <param name="timeProvider">The source of the current time.</param>
    public ProductService(IProductRepository repository, TimeProvider timeProvider)
    {
        _repository = repository;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedList<Product>> GetPagedAsync(GetPagedProductsQuery query, CancellationToken cancellationToken)
    {
        return _repository.GetPagedAsync(query.PageNumber, query.PageSize, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<Product>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Product? product = await _repository.FindByIdAsync(id, cancellationToken);

        return product is null
            ? Result.Fail(new NotFoundError(PRODUCT_NOT_FOUND))
            : Result.Ok(product);
    }

    /// <inheritdoc />
    public async Task<Result<Product>> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        Result<Product> creation = ProductFactory.Create(command, _timeProvider.GetUtcNow());
        if (creation.IsFailed)
        {
            return creation;
        }

        await _repository.AddAsync(creation.Value, cancellationToken);
        return creation;
    }

    /// <inheritdoc />
    public async Task<Result<Product>> UpdateDescriptionAsync(int id, string? description,
        CancellationToken cancellationToken)
    {
        Result validation = ProductRules.ValidateDescription(description);
        if (validation.IsFailed)
        {
            return validation;
        }

        Product? product = await _repository.FindByIdAsync(id, cancellationToken);
        if (product is null)
        {
            return Result.Fail(new NotFoundError(PRODUCT_NOT_FOUND));
        }

        product.ChangeDescription(description ?? string.Empty, _timeProvider.GetUtcNow());
        await _repository.UpdateAsync(product, cancellationToken);
        return Result.Ok(product);
    }
}
