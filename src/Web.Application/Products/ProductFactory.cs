using FluentResults;
using Web.Domain.Entities;

namespace Web.Application.Products;

/// <summary>
///     Creates <see cref="Product" /> instances from a <see cref="CreateProductCommand" /> after checking the rules.
/// </summary>
public static class ProductFactory
{
    /// <summary>
    ///     Checks the command and creates a new product.
    /// </summary>
    /// <param name="command">The command containing product details.</param>
    /// <param name="createdAt">The time of creation.</param>
    /// <returns>A successful result with the new product, or a failed result with the first broken rule.</returns>
    public static Result<Product> Create(CreateProductCommand command, DateTimeOffset createdAt)
    {
        Result check = Result.Merge(
            ProductRules.ValidateName(command.Name),
            ProductRules.ValidatePrice(command.Price),
            ProductRules.ValidateImgUri(command.ImgUri),
            ProductRules.ValidateDescription(command.Description));

        if (check.IsFailed)
        {
            return check;
        }

        return Result.Ok(new Product(
            command.Name,
            command.ImgUri,
            command.Price,
            command.Description ?? string.Empty,
            createdAt));
    }
}
