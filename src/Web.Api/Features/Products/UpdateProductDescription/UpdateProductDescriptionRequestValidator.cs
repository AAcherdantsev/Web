using FastEndpoints;
using Web.Api.Validation;

namespace Web.Api.Features.Products.UpdateProductDescription;

/// <summary>
///     Validates <see cref="UpdateProductDescriptionRequest" /> before the endpoint handles it.
/// </summary>
public sealed class UpdateProductDescriptionRequestValidator : Validator<UpdateProductDescriptionRequest>
{
    /// <summary>
    ///     Initializes the validation rules. An empty description is allowed, so only a missing value is rejected.
    /// </summary>
    public UpdateProductDescriptionRequestValidator()
    {
        RuleFor(x => x.Description).ProductNewDescriptionRules();
    }
}
