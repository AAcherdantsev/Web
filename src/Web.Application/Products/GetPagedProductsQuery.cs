namespace Web.Application.Products;

/// <summary>
///     Query for retrieving a paginated list of products.
/// </summary>
public class GetPagedProductsQuery
{
    /// <summary>
    ///     The zero-based page number.
    /// </summary>
    public int PageNumber { get; init; }

    /// <summary>
    ///     The number of products per page.
    /// </summary>
    public int PageSize { get; init; }
}
