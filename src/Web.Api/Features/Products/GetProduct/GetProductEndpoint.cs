using FastEndpoints;
using FluentResults;
using Mapster;
using Web.Api.Configuration.RateLimiting;
using Web.Api.Errors;
using Web.Api.Extensions;
using Web.Api.Models;
using Web.Application.Common.Errors;
using Web.Domain.Entities;

using Web.Application.Products;

namespace Web.Api.Features.Products.GetProduct;

/// <summary>
///     Endpoint for retrieving a single product by its ID.
/// </summary>
public sealed class GetProductEndpoint : EndpointWithoutRequest<ProductDto>
{
    private readonly IProductService _productService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GetProductEndpoint" /> class.
    /// </summary>
    /// <param name="productService">The repository used to access product data.</param>
    public GetProductEndpoint(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    ///     Configures the route, version, and Swagger summary for the endpoint.
    /// </summary>
    public override void Configure()
    {
        Get("/{id:int}");
        Version(1);
        Group<ProductGroup>();

        Summary(s =>
        {
            s.Summary = "Get product by ID";
            s.Description = "Returns a single product";
            s.Params["id"] = "Product ID";
            s.Response<ProductDto>(StatusCodes.Status200OK, "Product found");
            s.Response(StatusCodes.Status404NotFound, "Product not found");
            s.AddErrorResponses();
        });

        Options(b => b.RequireRateLimiting(RateLimitingPolicies.READS));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        int productId = Route<int>("id");
        Result<Product> result = await _productService.GetByIdAsync(productId, ct);

        if (result.IsSuccess)
        {
            await Send.ResponseAsync(result.Value.Adapt<ProductDto>(), cancellation: ct);
            return;
        }

        if (result.HasError<NotFoundError>())
        {
            await Send.ResultAsync(ProblemResults.NotFound(result.Errors[0].Message));
            return;
        }

        throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Message)));
    }
}
