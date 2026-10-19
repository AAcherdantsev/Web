using System.Globalization;
using FluentResults;
using Web.Domain.Entities;

namespace Web.Application.Products;

/// <summary>
///     The business rules for the fields of a product. The HTTP validators use the same limits, so the rules are
///     defined once, and every use case checks them.
/// </summary>
public static class ProductRules
{
    /// <summary>
    ///     Checks the product name.
    /// </summary>
    public static Result ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Fail("Name cannot be empty.");
        }

        if (name.Length > ProductLimits.MAX_NAME_LENGTH)
        {
            return Result.Fail($"Name cannot be longer than {ProductLimits.MAX_NAME_LENGTH} characters.");
        }

        return Result.Ok();
    }

    /// <summary>
    ///     Checks the price: greater than 0, at most <see cref="ProductPrice.MAX" />, and with at most two decimal places.
    /// </summary>
    public static Result ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            return Result.Fail("Price must be greater than 0.");
        }

        if (price > ProductPrice.MAX || !ProductPrice.HasAtMostTwoDecimalPlaces(price))
        {
            return Result.Fail(
                $"Price must be at most {ProductPrice.MAX.ToString(CultureInfo.InvariantCulture)} and have at most 2 decimal places.");
        }

        return Result.Ok();
    }

    /// <summary>
    ///     Checks the image address.
    /// </summary>
    public static Result ValidateImgUri(string? imgUri)
    {
        if (!ProductImageUri.IsValid(imgUri))
        {
            return Result.Fail("Image URI is not valid.");
        }

        if (imgUri!.Length > ProductLimits.MAX_IMG_URI_LENGTH)
        {
            return Result.Fail($"Image URI cannot be longer than {ProductLimits.MAX_IMG_URI_LENGTH} characters.");
        }

        return Result.Ok();
    }

    /// <summary>
    ///     Checks the description. A missing description is allowed and stored as empty.
    /// </summary>
    public static Result ValidateDescription(string? description)
    {
        if (description?.Length > ProductLimits.MAX_DESCRIPTION_LENGTH)
        {
            return Result.Fail(
                $"Description cannot be longer than {ProductLimits.MAX_DESCRIPTION_LENGTH} characters.");
        }

        return Result.Ok();
    }
}
