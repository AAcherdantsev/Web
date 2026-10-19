using FluentValidation;

namespace Web.Application.Products;

public sealed class UpdateProductDescriptionValidator : AbstractValidator<UpdateProductDescriptionCommand>
{
    public UpdateProductDescriptionValidator()
    {
        RuleFor(x => x.Description).ProductDescriptionRules();
    }
}
