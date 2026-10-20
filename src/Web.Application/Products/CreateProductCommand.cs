namespace Web.Application.Products;

/// <summary>
///     Command to create a new product with the specified properties.
/// </summary>
public sealed record CreateProductCommand
{
    /// <summary>
    ///     The name of the product to be created.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The URI of the image associated with the product.
    /// </summary>
    public required string ImgUri { get; init; }

    /// <summary>
    ///     The price of the product to be created.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    ///     The description of the product to be created.
    /// </summary>
    public string? Description { get; init; }
}
