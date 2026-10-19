using FastEndpoints;
using FluentResults;
using Mapster;
using Web.Api.Configuration.RateLimiting;
using Web.Api.Errors;
using Web.Api.Extensions;
using Web.Api.Features.Products.GetProduct;
using Web.Api.Models;
using Web.Application.Products;
using Web.Domain.Entities;

namespace Web.Api.Features.Products.CreateProduct;

/// <summary>
///     Endpoint for creating a new product.
/// </summary>
public sealed class CreateProductEndpoint : Endpoint<CreateProductRequest, ProductDto>
{
    private readonly IProductService _productService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CreateProductEndpoint" /> class.
    /// </summary>
    /// <param name="productService">The product service used to create and store product data.</param>
    public CreateProductEndpoint(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    ///     Configures the POST endpoint route, version, Swagger documentation, validation, and access.
    /// </summary>
    public override void Configure()
    {
        Post("/");
        Version(1);
        Group<ProductGroup>();

        Summary(s =>
        {
            s.Summary = "Create a new product";
            s.Description = "Creates a new product. The response contains the created product and a Location header.";
            s.ExampleRequest = new()
            {
                Price = 100,
                Name = "Phone",
                ImgUri = "https://example.com/phone.jpg",
                Description = "A sample product for demonstration."
            };
            s.Response<ProductDto>(StatusCodes.Status201Created, "Product successfully created");
            s.Response(StatusCodes.Status400BadRequest, "Invalid request data");

            s.AddErrorResponses();
        });

        Validator<CreateProductRequestValidator>();
        Options(b => b.RequireRateLimiting(RateLimitingPolicies.WRITES));
    }

    /// <summary>
    ///     Handles the POST request to create a new product.
    /// </summary>
    /// <param name="req">The incoming create a product request.</param>
    /// <param name="ct">The cancellation token.</param>
    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        CreateProductCommand command = new()
        {
            Name = req.Name, ImgUri = req.ImgUri, Price = req.Price, Description = req.Description
        };

        Result<Product> result = await _productService.CreateAsync(command, ct);

        if (result.IsFailed)
        {
            await Send.ResultAsync(ProblemResults.BadRequest(result.Errors));
            return;
        }

        ProductDto dto = result.Value.Adapt<ProductDto>();
        await Send.CreatedAtAsync<GetProductEndpoint>(new { id = dto.Id }, dto, cancellation: ct);
    }
}
