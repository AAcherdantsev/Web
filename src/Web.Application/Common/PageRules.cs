namespace Web.Application.Common;

/// <summary>
///     The rules for the page number and the page size. The request validators and the application services both check
///     them, so each rule is defined once here.
/// </summary>
public static class PageRules
{
    /// <summary>The page number is negative.</summary>
    public const string PAGE_NUMBER_NEGATIVE = "The page number must be greater than or equal to 0.";

    /// <summary>The page size is zero or negative.</summary>
    public const string PAGE_SIZE_TOO_SMALL = "The page size must be at least 1.";

    /// <summary>The offset of the page does not fit in an <see cref="int" />.</summary>
    public const string PAGE_OFFSET_TOO_LARGE = "The requested page is beyond the supported range.";

    /// <summary>
    ///     The message for a page size above the configured maximum.
    /// </summary>
    public static string PageSizeTooLarge(int maxPageSize)
    {
        return $"The page size must not be greater than {maxPageSize}.";
    }

    /// <summary>
    ///     Whether the page number is zero or greater.
    /// </summary>
    public static bool IsPageNumberNonNegative(int pageNumber)
    {
        return pageNumber >= 0;
    }

    /// <summary>
    ///     Whether the page size is at least one.
    /// </summary>
    public static bool IsPageSizeAtLeastOne(int pageSize)
    {
        return pageSize >= 1;
    }

    /// <summary>
    ///     Whether the page size is at most <paramref name="maxPageSize" />.
    /// </summary>
    public static bool IsPageSizeAtMost(int pageSize, int maxPageSize)
    {
        return pageSize <= maxPageSize;
    }

    /// <summary>
    ///     Whether the offset of the page (<c>pageNumber × pageSize</c>) fits in an <see cref="int" />, which is the type
    ///     of the offset in the database query.
    /// </summary>
    public static bool IsOffsetWithinInt(int pageNumber, int pageSize)
    {
        return (long) pageNumber * pageSize <= int.MaxValue;
    }
}
