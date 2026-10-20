namespace Web.Domain.Entities;

/// <summary>
///     The business rules of a product. Each rule is defined once here, as a predicate and a message. The entity uses
///     the predicates when a product is created or changed, and the request validators in the Application project use
///     the same predicates, so the two layers cannot disagree.
/// </summary>
public static class ProductRules
{
    /// <summary>The name is empty or only white space.</summary>
    public const string NAME_EMPTY = "The name must not be empty.";

    /// <summary>The name is longer than <see cref="ProductLimits.MAX_NAME_LENGTH" />.</summary>
    public static readonly string NAME_TOO_LONG =
        $"The name must not be longer than {ProductLimits.MAX_NAME_LENGTH} characters.";

    /// <summary>The price is zero or negative.</summary>
    public const string PRICE_NOT_POSITIVE = "The price must be greater than 0.";

    /// <summary>The price is larger than <see cref="ProductPrice.MAX" />.</summary>
    public static readonly string PRICE_TOO_LARGE =
        $"The price must be at most {ProductPrice.MAX.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

    /// <summary>The price has more than two decimal places.</summary>
    public const string PRICE_TOO_MANY_DECIMALS = "The price must have at most 2 decimal places.";

    /// <summary>The image address is not an absolute <c>http</c> or <c>https</c> address.</summary>
    public const string IMG_URI_INVALID = "The image URI is not valid.";

    /// <summary>The image address is longer than <see cref="ProductLimits.MAX_IMG_URI_LENGTH" />.</summary>
    public static readonly string IMG_URI_TOO_LONG =
        $"The image URI must not be longer than {ProductLimits.MAX_IMG_URI_LENGTH} characters.";

    /// <summary>The description is longer than <see cref="ProductLimits.MAX_DESCRIPTION_LENGTH" />.</summary>
    public static readonly string DESCRIPTION_TOO_LONG =
        $"The description must not be longer than {ProductLimits.MAX_DESCRIPTION_LENGTH} characters.";

    /// <summary>Whether the name is provided. A name of only white space is not provided.</summary>
    public static bool IsNameProvided(string? name)
    {
        return !string.IsNullOrWhiteSpace(name);
    }

    /// <summary>Whether the name is within <see cref="ProductLimits.MAX_NAME_LENGTH" />. A missing name is within it.</summary>
    public static bool IsNameWithinLimit(string? name)
    {
        return name is not { Length: > ProductLimits.MAX_NAME_LENGTH };
    }

    /// <summary>Whether the price is greater than zero.</summary>
    public static bool IsPricePositive(decimal price)
    {
        return price > 0;
    }

    /// <summary>Whether the price fits the <c>decimal(18,2)</c> column.</summary>
    public static bool IsPriceWithinLimit(decimal price)
    {
        return price <= ProductPrice.MAX;
    }

    /// <summary>Whether the image address is a valid absolute <c>http</c> or <c>https</c> address.</summary>
    public static bool IsImgUriValid(string? imgUri)
    {
        return ProductImageUri.IsValid(imgUri);
    }

    /// <summary>Whether the image address is within <see cref="ProductLimits.MAX_IMG_URI_LENGTH" />.</summary>
    public static bool IsImgUriWithinLimit(string? imgUri)
    {
        return imgUri is not { Length: > ProductLimits.MAX_IMG_URI_LENGTH };
    }

    /// <summary>Whether the description is within <see cref="ProductLimits.MAX_DESCRIPTION_LENGTH" />.</summary>
    public static bool IsDescriptionWithinLimit(string? description)
    {
        return description is not { Length: > ProductLimits.MAX_DESCRIPTION_LENGTH };
    }

    /// <summary>
    ///     Checks all the values of a product and returns the messages of the broken rules. An empty list means that
    ///     the values are valid.
    /// </summary>
    public static IReadOnlyList<string> Violations(string? name, string? imgUri, decimal price, string? description)
    {
        List<string> violations = [];

        if (!IsNameProvided(name))
        {
            violations.Add(NAME_EMPTY);
        }

        if (!IsNameWithinLimit(name))
        {
            violations.Add(NAME_TOO_LONG);
        }

        if (!IsPricePositive(price))
        {
            violations.Add(PRICE_NOT_POSITIVE);
        }

        if (!IsPriceWithinLimit(price))
        {
            violations.Add(PRICE_TOO_LARGE);
        }

        if (!ProductPrice.HasAtMostTwoDecimalPlaces(price))
        {
            violations.Add(PRICE_TOO_MANY_DECIMALS);
        }

        if (!IsImgUriValid(imgUri))
        {
            violations.Add(IMG_URI_INVALID);
        }

        if (!IsImgUriWithinLimit(imgUri))
        {
            violations.Add(IMG_URI_TOO_LONG);
        }

        violations.AddRange(DescriptionViolations(description));
        return violations;
    }

    /// <summary>
    ///     Checks a description and returns the messages of the broken rules. A missing description is valid, because
    ///     it is stored as an empty string.
    /// </summary>
    public static IReadOnlyList<string> DescriptionViolations(string? description)
    {
        return IsDescriptionWithinLimit(description) ? [] : [DESCRIPTION_TOO_LONG];
    }
}
