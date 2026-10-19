using FastEndpoints;
using Mapster;
using Web.Api.Configuration.RateLimiting;
using Web.Api.Extensions;
using Web.Api.Models;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Api.Features.Products.GetAllProducts;

/// <summary>
///     Handles requests to retrieve all products.
/// </summary>
public sealed class GetAllProductsEndpoint : EndpointWithoutRequest<List<ProductDto>>
{
    private readonly IProductService _productService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetAllProductsEndpoint" /> class.
    /// </summary>
    /// <param name="productService">The repository used to access product data.</param>
    public GetAllProductsEndpoint(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    ///     Configures the endpoint route, version, and Swagger documentation.
    /// </summary>
    public override void Configure()
    {
        Get("/");
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
        IReadOnlyList<Product> products = await _productService.GetAllAsync(ct);

        await Send.ResponseAsync(products.Adapt<List<ProductDto>>(), cancellation: ct);
    }
}
