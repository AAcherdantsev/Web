namespace Web.Api.Features.Products.UpdateProductDescription;

/// <summary>
///     Represents a request to update the description of a product.
/// </summary>
public sealed class UpdateProductDescriptionRequest
{
    /// <summary>
    ///     The new description of the product.
    /// </summary>
    public required string Description { get; init; }
}
