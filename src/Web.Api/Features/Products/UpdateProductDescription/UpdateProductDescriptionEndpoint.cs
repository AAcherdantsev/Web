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

namespace Web.Api.Features.Products.UpdateProductDescription;

/// <summary>
///     Endpoint for updating the description of a product by ID.
/// </summary>
public sealed class UpdateProductDescriptionEndpoint : Endpoint<UpdateProductDescriptionRequest, ProductDto>
{
    private readonly IProductService _productService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UpdateProductDescriptionEndpoint" /> class.
    /// </summary>
    /// <param name="productService">The product service used to access and update product data.</param>
    public UpdateProductDescriptionEndpoint(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    ///     Configures the endpoint route, version, Swagger documentation, validation, and access.
    /// </summary>
    public override void Configure()
    {
        Patch("/{id:int}");
        Version(1);
        Group<ProductGroup>();

        Summary(s =>
        {
            s.Summary = "Update product description by ID";
            s.Description = "Updates the description of a product by its ID.";
            s.Params["id"] = "Product ID";
            s.ExampleRequest = new() { Description = "An updated description of the product." };
            s.Response<ProductDto>(StatusCodes.Status200OK, "Product updated successfully");
            s.Response(StatusCodes.Status400BadRequest, "Invalid request data");
            s.Response(StatusCodes.Status404NotFound, "Product not found");
            s.AddErrorResponses();
        });

        Validator<UpdateProductDescriptionRequestValidator>();

        Options(b => b.RequireRateLimiting(RateLimitingPolicies.WRITES));
    }

    /// <summary>
    ///     Handles the PATCH request to update a product's description.
    /// </summary>
    /// <param name="req">The incoming request containing the new description.</param>
    /// <param name="ct">The cancellation token.</param>
    public override async Task HandleAsync(UpdateProductDescriptionRequest req, CancellationToken ct)
    {
        int productId = Route<int>("id");

        UpdateProductDescriptionCommand command = new()
        {
            Id = productId,
            Description = req.Description
        };

        Result<Product> result = await _productService.UpdateDescriptionAsync(command, ct);

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

        await Send.ResultAsync(ProblemResults.BadRequest(result.Errors));
    }
}
