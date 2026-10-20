namespace Web.Application.Common.Repositories;

/// <summary>
///     Thrown by a product store when a product cannot be saved because it changed after it was read, or because it
///     no longer exists. Every store throws this exception for the same situation.
/// </summary>
public sealed class ProductConflictException : Exception
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ProductConflictException" /> class.
    /// </summary>
    /// <param name="productId">The identifier of the product that could not be saved.</param>
    public ProductConflictException(int productId)
        : base($"The product with ID {productId} was changed by another request.")
    {
    }
}
