using Web.Domain.Entities;

namespace Web.Application.Products;

/// <summary>
///     The product rules as the request validators in Web.Api use them. The rules are defined once, in the domain
///     (<see cref="ProductRules" />), and this class only makes them available. Web.Api does not reference the domain
///     project, so it reads the rules from here.
/// </summary>
public static class ProductValidation
{
    /// <inheritdoc cref="ProductRules.NAME_EMPTY" />
    public const string NAME_EMPTY = ProductRules.NAME_EMPTY;

    /// <inheritdoc cref="ProductRules.NAME_TOO_LONG" />
    public static readonly string NAME_TOO_LONG = ProductRules.NAME_TOO_LONG;

    /// <inheritdoc cref="ProductRules.PRICE_NOT_POSITIVE" />
    public const string PRICE_NOT_POSITIVE = ProductRules.PRICE_NOT_POSITIVE;

    /// <inheritdoc cref="ProductRules.PRICE_TOO_LARGE" />
    public static readonly string PRICE_TOO_LARGE = ProductRules.PRICE_TOO_LARGE;

    /// <inheritdoc cref="ProductRules.PRICE_TOO_MANY_DECIMALS" />
    public const string PRICE_TOO_MANY_DECIMALS = ProductRules.PRICE_TOO_MANY_DECIMALS;

    /// <inheritdoc cref="ProductRules.IMG_URI_INVALID" />
    public const string IMG_URI_INVALID = ProductRules.IMG_URI_INVALID;

    /// <inheritdoc cref="ProductRules.IMG_URI_TOO_LONG" />
    public static readonly string IMG_URI_TOO_LONG = ProductRules.IMG_URI_TOO_LONG;

    /// <inheritdoc cref="ProductRules.DESCRIPTION_TOO_LONG" />
    public static readonly string DESCRIPTION_TOO_LONG = ProductRules.DESCRIPTION_TOO_LONG;

    /// <inheritdoc cref="ProductRules.IsNameProvided" />
    public static bool IsNameProvided(string? name) => ProductRules.IsNameProvided(name);

    /// <inheritdoc cref="ProductRules.IsNameWithinLimit" />
    public static bool IsNameWithinLimit(string? name) => ProductRules.IsNameWithinLimit(name);

    /// <inheritdoc cref="ProductRules.IsPricePositive" />
    public static bool IsPricePositive(decimal price) => ProductRules.IsPricePositive(price);

    /// <inheritdoc cref="ProductRules.IsPriceWithinLimit" />
    public static bool IsPriceWithinLimit(decimal price) => ProductRules.IsPriceWithinLimit(price);

    /// <inheritdoc cref="ProductPrice.HasAtMostTwoDecimalPlaces" />
    public static bool HasAtMostTwoDecimalPlaces(decimal price) => ProductPrice.HasAtMostTwoDecimalPlaces(price);

    /// <inheritdoc cref="ProductRules.IsImgUriValid" />
    public static bool IsImgUriValid(string? imgUri) => ProductRules.IsImgUriValid(imgUri);

    /// <inheritdoc cref="ProductRules.IsImgUriWithinLimit" />
    public static bool IsImgUriWithinLimit(string? imgUri) => ProductRules.IsImgUriWithinLimit(imgUri);

    /// <inheritdoc cref="ProductRules.IsDescriptionWithinLimit" />
    public static bool IsDescriptionWithinLimit(string? description) =>
        ProductRules.IsDescriptionWithinLimit(description);
}
