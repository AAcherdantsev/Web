namespace Web.Application.Common;

/// <summary>
///     Represents one page of a list, with the information a client needs to request the other pages.
/// </summary>
/// <typeparam name="T">The type of elements contained in the list.</typeparam>
public sealed class PagedList<T>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PagedList{T}" /> class with the specified values, total count, page
    ///     number, and page size.
    /// </summary>
    /// <param name="values">The items on the current page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <param name="page">The current page number, zero-based.</param>
    /// <param name="pageSize">The size of each page.</param>
    public PagedList(IEnumerable<T> values, long totalCount, int page, int pageSize)
    {
        Values = new List<T>(values).AsReadOnly();
        TotalCount = totalCount;
        PageNumber = page;
        PageSize = pageSize;
    }

    /// <summary>
    ///     The current page (zero-based).
    /// </summary>
    public int PageNumber { get; }

    /// <summary>
    ///     The page size.
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    ///     The total number of elements across all pages.
    /// </summary>
    public long TotalCount { get; }

    /// <summary>
    ///     The number of pages. Zero when there are no elements.
    /// </summary>
    public long TotalPages => PageSize <= 0 ? 0 : (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    ///     Whether a page follows the current one.
    /// </summary>
    public bool HasNext => PageNumber + 1 < TotalPages;

    /// <summary>
    ///     The values of the current page.
    /// </summary>
    public IReadOnlyCollection<T> Values { get; }
}
