namespace Web.Application.Products;

/// <summary>
/// Represents a command to update the description of a product.
/// </summary>
public sealed record UpdateProductDescriptionCommand
{
    /// <summary>
    ///     The unique identifier of the product.
    /// </summary>
    public required int Id { get; init; }

    /// <summary>
    ///     The description of the product to be updated.
    /// </summary>
    public string? Description { get; init; }
}
