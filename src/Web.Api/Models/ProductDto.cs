namespace Web.Api.Models;

/// <summary>
///     Represents a product as returned by the API.
/// </summary>
public sealed class ProductDto
{
    /// <summary>
    ///     The unique identifier of the product.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    ///     The name of the product.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The URI of the product image.
    /// </summary>
    public required string ImgUri { get; init; }

    /// <summary>
    ///     The price of the product.
    /// </summary>
    public required decimal Price { get; init; }

    /// <summary>
    ///     The description of the product. Empty when the product has no description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    ///     The date and time when the product was created.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     The date and time when the product was last updated.
    /// </summary>
    public required DateTimeOffset LastUpdatedAt { get; init; }
}
