using FluentValidation;

namespace Web.Application.Products;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).ProductNameRules();
        RuleFor(x => x.Price).ProductPriceRules();
        RuleFor(x => x.ImgUri).ProductImageUrlRules();
        RuleFor(x => x.Description).ProductDescriptionRules();
    }
}
