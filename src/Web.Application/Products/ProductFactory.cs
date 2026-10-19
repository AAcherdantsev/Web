using FluentResults;
using FluentValidation.Results;
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
        ValidationResult validation = new CreateProductCommandValidator().Validate(command);

        if (!validation.IsValid)
        {
            return Result.Fail(validation.Errors.Select(e => e.ErrorMessage));
        }

        return Result.Ok(new Product(
            command.Name,
            command.ImgUri,
            command.Price,
            command.Description ?? string.Empty,
            createdAt));
    }
}
