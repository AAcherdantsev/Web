using Web.Application.Common.Repositories;
using Web.Domain.Entities;

namespace Web.Infrastructure.Persistence;

/// <summary>
///     The stored form of a product. Entity Framework Core maps this class to the <c>Products</c> table, and the
///     domain entity <see cref="Product" /> stays free of persistence concerns. The mock store keeps the same records.
/// </summary>
internal sealed class ProductRecord
{
    /// <summary>
    ///     The unique identifier, assigned by the store.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///     The product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     URI of the product image.
    /// </summary>
    public string ImgUri { get; set; } = string.Empty;

    /// <summary>
    ///     Price of the product.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    ///     Description of the product.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Date and time when the product was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    ///     Date and time of the last change.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; }

    /// <summary>
    ///     The version of the row. The database changes it on every update, and an update is saved only when it still
    ///     matches the version that was read.
    /// </summary>
    public byte[] RowVersion { get; set; } = [];

    /// <summary>
    ///     Creates the record of a domain product. The version is empty for a product that is not stored yet.
    /// </summary>
    /// <param name="product">The product.</param>
    /// <param name="rowVersion">The version the product was read at, or empty for a new product.</param>
    public static ProductRecord FromDomain(Product product, byte[] rowVersion)
    {
        return new()
        {
            Id = product.Id,
            Name = product.Name,
            ImgUri = product.ImgUri,
            Price = product.Price,
            Description = product.Description,
            CreatedAt = product.CreatedAt,
            LastUpdatedAt = product.LastUpdatedAt,
            RowVersion = rowVersion
        };
    }

    /// <summary>
    ///     Creates the domain product from the stored values. The values are checked against the domain rules.
    /// </summary>
    public Product ToDomain()
    {
        return Product.Restore(Id, Name, ImgUri, Price, Description, CreatedAt, LastUpdatedAt);
    }

    /// <summary>
    ///     Creates the domain product together with the version it was read at.
    /// </summary>
    public VersionedProduct ToVersioned()
    {
        return new(ToDomain(), RowVersion);
    }
}
