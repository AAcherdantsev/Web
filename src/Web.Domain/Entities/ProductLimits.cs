namespace Web.Domain.Entities;

/// <summary>
///     Maximum lengths of product text fields. The database schema, the request validators, and the business rules all use these values.
/// </summary>
public static class ProductLimits
{
    /// <summary>
    ///     The maximum length of <see cref="Product.Name" />.
    /// </summary>
    public const int MAX_NAME_LENGTH = 200;

    /// <summary>
    ///     The maximum length of <see cref="Product.ImgUri" />.
    /// </summary>
    public const int MAX_IMG_URI_LENGTH = 2048;

    /// <summary>
    ///     The maximum length of <see cref="Product.Description" />.
    /// </summary>
    public const int MAX_DESCRIPTION_LENGTH = 4000;
}
