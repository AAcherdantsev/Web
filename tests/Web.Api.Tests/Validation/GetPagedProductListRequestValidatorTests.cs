using FluentValidation.TestHelper;
using Microsoft.Extensions.Options;
using Web.Api.Configuration;
using Web.Api.Features.Products.GetPagedProductList;

namespace Web.Api.Tests.Validation;

[TestFixture]
public class GetPagedProductListRequestValidatorTests
{
    private readonly GetPagedProductListRequestValidator _validator = new(
        Options.Create(new PaginationOptions { DefaultPageSize = 10, MaxPageSize = 100 }));

    [Test]
    public void OmittedPageSize_HasNoErrors()
    {
        _validator.TestValidate(new GetPagedProductListRequest()).ShouldNotHaveAnyValidationErrors();
    }

    [TestCase(1)]
    [TestCase(100)]
    public void PageSize_WithinLimits_HasNoErrors(int pageSize)
    {
        _validator.TestValidate(new GetPagedProductListRequest { PageSize = pageSize })
            .ShouldNotHaveAnyValidationErrors();
    }

    [TestCase(0)]
    [TestCase(-5)]
    [TestCase(101)]
    public void PageSize_OutsideLimits_HasError(int pageSize)
    {
        _validator.TestValidate(new GetPagedProductListRequest { PageSize = pageSize })
            .ShouldHaveValidationErrorFor(x => x.PageSize);
    }

    [TestCase(0)]
    [TestCase(5)]
    public void PageNumber_WithValidValue_HasNoErrors(int pageNumber)
    {
        _validator.TestValidate(new GetPagedProductListRequest { PageNumber = pageNumber })
            .ShouldNotHaveValidationErrorFor(x => x.PageNumber);
    }

    [TestCase(-1)]
    [TestCase(-2)]
    public void PageNumber_Negative_HasError(int pageNumber)
    {
        _validator.TestValidate(new GetPagedProductListRequest { PageNumber = pageNumber })
            .ShouldHaveValidationErrorFor(x => x.PageNumber);
    }
}
