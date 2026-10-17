using FastEndpoints;
using FluentResults;
using Mapster;
using Web.Api.Configuration.RateLimiting;
using Web.Api.Extensions;
using Web.Api.Models;
using Web.Application.Common.Repositories;
using Web.Domain.Entities;

namespace Web.Api.Features.Products.GetAllProducts;

/// <summary>
///     Handles requests to retrieve all products.
/// </summary>
public sealed class GetAllProductsEndpoint : EndpointWithoutRequest<List<ProductDto>>
{
    private readonly IProductRepository _productRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetAllProductsEndpoint" /> class.
    /// </summary>
    /// <param name="productRepository">The repository used to access product data.</param>
    public GetAllProductsEndpoint(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    ///     Configures the endpoint route, version, and Swagger documentation.
    /// </summary>
    public override void Configure()
    {
        Get();
        Version(1);
        Group<ProductGroup>();

        Summary(s =>
        {
            s.Summary = "Get all products";
            s.Description = "Returns a list of all available products.";
            s.Response<List<ProductDto>>(StatusCodes.Status200OK, "List of products retrieved successfully");
            s.AddErrorResponses();
        });

        Options(b => b.RequireRateLimiting(RateLimitingPolicies.READS));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        Result<IEnumerable<Product>> result = await _productRepository.GetAllProductsAsync(ct);

        // Listing has no expected failure, so a failed result is an unexpected error (HTTP 500).
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Message)));
        }

        await Send.ResponseAsync(result.Value.Adapt<List<ProductDto>>(), cancellation: ct);
    }
}
