using FastEndpoints;
using Web.Application.Products;

namespace Web.Api.Features.Products.CreateProduct;

/// <summary>
///     Validates <see cref="CreateProductRequest" /> before the endpoint handles it.
/// </summary>
public sealed class CreateProductRequestValidator : Validator<CreateProductRequest>
{
    /// <summary>
    ///     Initializes the validation rules.
    /// </summary>
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).ProductNameRules();
        RuleFor(x => x.Price).ProductPriceRules();
        RuleFor(x => x.ImgUri).ProductImageUrlRules();
        RuleFor(x => x.Description).ProductDescriptionRules();
    }
}
