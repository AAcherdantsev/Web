using System.ComponentModel.DataAnnotations;

namespace Web.Api.Configuration;

/// <summary>
///     Pagination settings for the paged product list. Bound from the <c>Pagination</c> configuration section.
/// </summary>
public sealed class PaginationOptions : IValidatableObject
{
    /// <summary>
    ///     The configuration section name.
    /// </summary>
    public const string SECTION_NAME = "Pagination";

    /// <summary>
    ///     The page size used when a request does not specify one.
    /// </summary>
    [Range(1, 1000)]
    public int DefaultPageSize { get; set; } = 10;

    /// <summary>
    ///     The largest page size a client may request.
    /// </summary>
    [Range(1, 1000)]
    public int MaxPageSize { get; set; } = 100;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DefaultPageSize > MaxPageSize)
        {
            yield return new(
                $"{nameof(DefaultPageSize)} must not be greater than {nameof(MaxPageSize)}.",
                [nameof(DefaultPageSize), nameof(MaxPageSize)]);
        }
    }
}
