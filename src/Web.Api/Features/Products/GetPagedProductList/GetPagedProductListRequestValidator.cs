using FastEndpoints;
using FluentValidation;
using Microsoft.Extensions.Options;
using Web.Api.Configuration;
using Web.Api.Validation;

namespace Web.Api.Features.Products.GetPagedProductList;

/// <summary>
///     Validates <see cref="GetPagedProductListRequest" /> before the endpoint handles it.
/// </summary>
public sealed class GetPagedProductListRequestValidator : Validator<GetPagedProductListRequest>
{
    /// <summary>
    ///     Initializes the validation rules using the configured pagination limits.
    /// </summary>
    /// <param name="pagination">The pagination settings.</param>
    public GetPagedProductListRequestValidator(IOptions<PaginationOptions> pagination)
    {
        RuleFor(x => x.PageNumber).PageNumberRules();

        // A missing page size falls back to the configured default, so only a supplied value is checked
        RuleFor(x => x.PageSize).PageSizeRules(pagination)
            .When(x => x.PageSize.HasValue);
    }
}
