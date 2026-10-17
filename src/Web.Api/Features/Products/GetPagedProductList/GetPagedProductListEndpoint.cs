using FastEndpoints;
using FluentResults;
using Mapster;
using Microsoft.Extensions.Options;
using Web.Api.Configuration;
using Web.Api.Configuration.RateLimiting;
using Web.Api.Extensions;
using Web.Api.Models;
using Web.Application.Common;
using Web.Application.Common.Repositories;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Api.Features.Products.GetPagedProductList;

/// <summary>
///     Endpoint for retrieving a paginated list of products.
/// </summary>
public sealed class GetPagedProductListEndpoint : Endpoint<GetPagedProductListRequest, PagedList<ProductDto>>
{
    private readonly PaginationOptions _pagination;
    private readonly IProductRepository _productRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetPagedProductListEndpoint" /> class.
    /// </summary>
    /// <param name="productRepository">The repository used to retrieve product data.</param>
    /// <param name="pagination">The pagination settings, including the default page size.</param>
    public GetPagedProductListEndpoint(IProductRepository productRepository, IOptions<PaginationOptions> pagination)
    {
        _productRepository = productRepository;
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
            s.Params["pageNumber"] = "Zero-based page number.";
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
            PageNumber = req.PageNumber, PageSize = req.PageSize ?? _pagination.DefaultPageSize
        };

        Result<PagedList<Product>> result = await _productRepository.GetAllProductsAsync(query, ct);

        // Listing has no expected failure, so a failed result is an unexpected error (HTTP 500).
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Message)));
        }

        await Send.ResponseAsync(result.Value.Adapt<PagedList<ProductDto>>(), cancellation: ct);
    }
}
