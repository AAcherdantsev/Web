namespace Web.Application.Common;

/// <summary>
///     Represents a paginated list of items.
/// </summary>
/// <typeparam name="T">The type of elements contained in the list.</typeparam>
public class PagedList<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PagedList{T}" /> class with the specified values, total count, page
    ///     number, and page size.
    /// </summary>
    /// <param name="values">The items on the current page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="pageSize">The size of each page.</param>
    public PagedList(IEnumerable<T> values, long totalCount, int page, int pageSize)
    {
        Values = new List<T>(values).AsReadOnly();
        TotalCount = totalCount;
        PageNumber = page;
        PageSize = pageSize;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PagedList{T}" /> class.
    /// </summary>
    private PagedList()
    {
    }

    /// <summary>
    ///     Gets an empty paged list.
    /// </summary>
    public static PagedList<T> Empty =>
        new() { PageNumber = 0, PageSize = 0, Values = [] };

    /// <summary>
    ///     The current page (zero-based)
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    ///     The page size
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    ///     The total number of elements across all pages.
    /// </summary>
    public long TotalCount { get; init; }

    /// <summary>
    ///     The values of the current page.
    /// </summary>
    public IReadOnlyCollection<T> Values { get; init; } = [];
}
