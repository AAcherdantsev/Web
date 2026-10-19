namespace Web.Api.Features.Products.GetPagedProductList;

/// <summary>
///     Represents a request for retrieving a paginated list of products.
/// </summary>
public sealed class GetPagedProductListRequest
{
    /// <summary>
    ///     The zero-based page number.
    /// </summary>
    public int? PageNumber { get; init; }

    /// <summary>
    ///     The number of products per page. When omitted, the configured default page size is used.
    /// </summary>
    public int? PageSize { get; init; }
}
