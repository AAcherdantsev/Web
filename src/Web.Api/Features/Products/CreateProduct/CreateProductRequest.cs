namespace Web.Api.Features.Products.CreateProduct;

/// <summary>
///     Represents a request to create a product with specified properties.
/// </summary>
public sealed class CreateProductRequest
{
    /// <summary>
    ///     The name of the product to be created.
    ///     This property is required and specifies the product's name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The URI of the image associated with the product.
    ///     This property is required and specifies the product's image.
    /// </summary>
    public required string ImgUri { get; init; }

    /// <summary>
    ///     The price of the product to be created. This property is required.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    ///     The description of the product to be created. This property is optional.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
