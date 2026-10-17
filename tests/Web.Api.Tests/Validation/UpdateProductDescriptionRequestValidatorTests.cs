using FluentValidation.TestHelper;
using Web.Api.Features.Products.UpdateProductDescription;

namespace Web.Api.Tests.Validation;

[TestFixture]
public class UpdateProductDescriptionRequestValidatorTests
{
    private readonly UpdateProductDescriptionRequestValidator _validator = new();

    [Test]
    public void EmptyDescription_IsAllowed()
    {
        _validator.TestValidate(new UpdateProductDescriptionRequest { Description = "" })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void NullDescription_HasError()
    {
        _validator.TestValidate(new UpdateProductDescriptionRequest { Description = null! })
            .ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Test]
    public void Description_LongerThanTheLimit_HasError()
    {
        _validator.TestValidate(new UpdateProductDescriptionRequest
            {
                Description = new('x', Domain.Entities.ProductLimits.MAX_DESCRIPTION_LENGTH + 1)
            })
            .ShouldHaveValidationErrorFor(x => x.Description);
    }
}
