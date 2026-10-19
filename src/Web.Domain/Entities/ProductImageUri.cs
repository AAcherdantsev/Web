namespace Web.Domain.Entities;

/// <summary>
///     Rules for the address of a product image. Used by the request validators and by the business rules.
/// </summary>
public static class ProductImageUri
{
    /// <summary>
    ///     Checks that the value is an absolute <c>http</c> or <c>https</c> address.
    /// </summary>
    /// <remarks>
    ///     Other schemes, such as <c>javascript:</c> or <c>file:</c>, are rejected, because clients display the image
    ///     address and could be sent to a URI that runs code or reads local files.
    /// </remarks>
    /// <param name="value">The address to check.</param>
    /// <returns><see langword="true" /> when the address is a valid image address; otherwise <see langword="false" />.</returns>
    public static bool IsValid(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
