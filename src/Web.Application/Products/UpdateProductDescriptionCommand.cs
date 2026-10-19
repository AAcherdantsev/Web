namespace Web.Application.Products;

public record UpdateProductDescriptionCommand
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
