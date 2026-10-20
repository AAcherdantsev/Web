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
    private const string PRODUCT_CHANGED = "The product was changed by another request. Try again.";

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
    public async Task<IReadOnlyList<ProductModel>> GetAllAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<Product> products = await _repository.GetAllAsync(cancellationToken);

        return products.Select(ToModel).ToList();
    }

    /// <inheritdoc />
    public async Task<Result<PagedList<ProductModel>>> GetPagedAsync(GetPagedProductsQuery query,
        CancellationToken cancellationToken)
    {
        // The API validates the same rules. The check is repeated here, because the offset must be an int in the store.
        if (!PageRules.IsPageNumberNonNegative(query.PageNumber))
        {
            return Result.Fail(new ValidationError(PageRules.PAGE_NUMBER_NEGATIVE));
        }

        if (!PageRules.IsPageSizeAtLeastOne(query.PageSize))
        {
            return Result.Fail(new ValidationError(PageRules.PAGE_SIZE_TOO_SMALL));
        }

        if (!PageRules.IsOffsetWithinInt(query.PageNumber, query.PageSize))
        {
            return Result.Fail(new ValidationError(PageRules.PAGE_OFFSET_TOO_LARGE));
        }

        PagedList<Product> page =
            await _repository.GetPagedAsync(query.PageNumber, query.PageSize, cancellationToken);

        return Result.Ok(new PagedList<ProductModel>(
            page.Values.Select(ToModel),
            page.TotalCount,
            page.PageNumber,
            page.PageSize));
    }

    /// <inheritdoc />
    public async Task<Result<ProductModel>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        VersionedProduct? stored = await _repository.FindByIdAsync(id, cancellationToken);

        return stored is null
            ? Result.Fail(new NotFoundError(PRODUCT_NOT_FOUND))
            : Result.Ok(ToModel(stored.Product));
    }

    /// <inheritdoc />
    public async Task<Result<ProductModel>> CreateAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        string description = command.Description ?? string.Empty;

        IReadOnlyList<string> violations =
            ProductRules.Violations(command.Name, command.ImgUri, command.Price, description);
        if (violations.Count > 0)
        {
            return Result.Fail(violations.Select(v => new ValidationError(v)));
        }

        Product product = new(command.Name, command.ImgUri, command.Price, description,
            _timeProvider.GetUtcNow());

        Product stored = await _repository.AddAsync(product, cancellationToken);
        return Result.Ok(ToModel(stored));
    }

    /// <inheritdoc />
    public async Task<Result<ProductModel>> UpdateDescriptionAsync(UpdateProductDescriptionCommand command,
        CancellationToken cancellationToken)
    {
        string description = command.Description ?? string.Empty;

        IReadOnlyList<string> violations = ProductRules.DescriptionViolations(description);
        if (violations.Count > 0)
        {
            return Result.Fail(violations.Select(v => new ValidationError(v)));
        }

        VersionedProduct? current = await _repository.FindByIdAsync(command.Id, cancellationToken);
        if (current is null)
        {
            return Result.Fail(new NotFoundError(PRODUCT_NOT_FOUND));
        }

        current.Product.ChangeDescription(description, _timeProvider.GetUtcNow());

        try
        {
            Product updated = await _repository.UpdateAsync(current, cancellationToken);
            return Result.Ok(ToModel(updated));
        }
        catch (ProductConflictException)
        {
            // Another request saved the product after it was read here. The client can read it again and retry.
            return Result.Fail(new ConflictError(PRODUCT_CHANGED));
        }
    }

    private static ProductModel ToModel(Product product)
    {
        return new()
        {
            Id = product.Id,
            Name = product.Name,
            ImgUri = product.ImgUri,
            Price = product.Price,
            Description = product.Description,
            CreatedAt = product.CreatedAt,
            LastUpdatedAt = product.LastUpdatedAt
        };
    }
}
