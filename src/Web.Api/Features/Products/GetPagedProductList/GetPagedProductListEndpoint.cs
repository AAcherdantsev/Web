using FastEndpoints;
using Mapster;
using Microsoft.Extensions.Options;
using Web.Api.Configuration;
using Web.Api.Configuration.RateLimiting;
using Web.Api.Extensions;
using Web.Api.Models;
using Web.Application.Common;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Api.Features.Products.GetPagedProductList;

/// <summary>
///     Endpoint for retrieving a paginated list of products.
/// </summary>
public sealed class GetPagedProductListEndpoint : Endpoint<GetPagedProductListRequest, PagedList<ProductDto>>
{
    private readonly PaginationOptions _pagination;
    private readonly IProductService _productService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetPagedProductListEndpoint" /> class.
    /// </summary>
    /// <param name="productService">The product service used to retrieve product data.</param>
    /// <param name="pagination">The pagination settings, including the default page size.</param>
    public GetPagedProductListEndpoint(IProductService productService, IOptions<PaginationOptions> pagination)
    {
        _productService = productService;
        _pagination = pagination.Value;
    }

    /// <summary>
    ///     Configures the endpoint route, version, Swagger documentation, validation, and access.
    /// </summary>
    public override void Configure()
    {
        Get("/");
        Version(2);
        Group<ProductGroup>();

        Summary(s =>
        {
            s.Summary = "Get paged list of products";
            s.Description =
                "Returns a paginated list of products. Uses the configured default page size when pageSize is omitted.";
            s.Params["pageNumber"] = "Zero-based page number. Defaults to 0.";
            s.Params["pageSize"] =
                "Number of products per page. Defaults to the configured default page size (10 by default).";
            s.Response<PagedList<ProductDto>>(StatusCodes.Status200OK, "Paged list of products retrieved successfully");
            s.Response(StatusCodes.Status400BadRequest, "Invalid pagination parameters");
            s.AddErrorResponses();
        });

        Validator<GetPagedProductListRequestValidator>();

        Options(b => b.RequireRateLimiting(RateLimitingPolicies.READS));
    }

    /// <summary>
    ///     Handles the GET request to retrieve a paginated list of products.
    /// </summary>
    /// <param name="req">The request containing pagination parameters.</param>
    /// <param name="ct">The cancellation token.</param>
    public override async Task HandleAsync(GetPagedProductListRequest req, CancellationToken ct)
    {
        GetPagedProductsQuery query = new()
        {
            PageNumber = req.PageNumber ?? 0,
            PageSize = req.PageSize ?? _pagination.DefaultPageSize
        };

        PagedList<Product> page = await _productService.GetPagedAsync(query, ct);

        await Send.ResponseAsync(page.Adapt<PagedList<ProductDto>>(), cancellation: ct);
    }
}
