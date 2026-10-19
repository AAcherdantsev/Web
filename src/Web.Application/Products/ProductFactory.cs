using FluentResults;
using Web.Domain.Entities;

namespace Web.Application.Products;

/// <summary>
///     Creates <see cref="Product" /> instances from a <see cref="CreateProductCommand" /> after validating them.
///     Shared by every repository implementation so the rules are defined once.
/// </summary>
public static class ProductFactory
{
    /// <summary>
    ///     Validates the command and creates a new product.
    /// </summary>
    /// <param name="command">The command containing product details.</param>
    /// <param name="utcNow">The current UTC time, used for the creation and update timestamps.</param>
    /// <returns>A successful result with the new product, or a failed result describing the invalid input.</returns>
    public static Result<Product> Create(CreateProductCommand command, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result.Fail("Name cannot be empty.");
        }

        if (command.Name.Length > ProductLimits.MAX_NAME_LENGTH)
        {
            return Result.Fail($"Name cannot be longer than {ProductLimits.MAX_NAME_LENGTH} characters.");
        }

        if (command.Price < 0)
        {
            return Result.Fail("Price cannot be negative.");
        }

        if (!ProductImageUri.IsValid(command.ImgUri))
        {
            return Result.Fail("Image URI is not valid.");
        }

        if (command.ImgUri.Length > ProductLimits.MAX_IMG_URI_LENGTH)
        {
            return Result.Fail($"Image URI cannot be longer than {ProductLimits.MAX_IMG_URI_LENGTH} characters.");
        }

        string description = command.Description ?? string.Empty;
        if (description.Length > ProductLimits.MAX_DESCRIPTION_LENGTH)
        {
            return Result.Fail($"Description cannot be longer than {ProductLimits.MAX_DESCRIPTION_LENGTH} characters.");
        }

        return Result.Ok(new Product
        {
            Name = command.Name,
            Price = command.Price,
            ImgUri = command.ImgUri,
            Description = description,
            CreatedAt = utcNow,
            LastUpdatedAt = utcNow
        });
    }
}
