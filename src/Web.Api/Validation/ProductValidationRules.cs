using FluentValidation;
using Web.Application.Common;
using Web.Application.Products;
using static Web.Api.Validation.ValidationErrorCodes;

namespace Web.Api.Validation;

/// <summary>
///     The request validation rules. Each rule calls a rule of the domain through <see cref="ProductValidation" /> or
///     <see cref="PageRules" />, so a rule is never written twice. The rules only add the field-level error and its code.
/// </summary>
public static class ProductValidationRules
{
    extension<T>(IRuleBuilder<T, string?> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string?> ProductNameRules()
        {
            return ruleBuilder.Must(ProductValidation.IsNameProvided).WithErrorCode(nameof(ERROR_0001))
                .WithMessage(ERROR_0001)
                .Must(ProductValidation.IsNameWithinLimit).WithErrorCode(nameof(ERROR_0002))
                .WithMessage(ERROR_0002);
        }

        public IRuleBuilderOptions<T, string?> ProductImageUrlRules()
        {
            return ruleBuilder.Must(ProductValidation.IsImgUriValid)
                .WithErrorCode(nameof(ERROR_0004))
                .WithMessage(ERROR_0004)
                .Must(ProductValidation.IsImgUriWithinLimit)
                .WithErrorCode(nameof(ERROR_0005))
                .WithMessage(ERROR_0005);
        }

        public IRuleBuilderOptions<T, string?> ProductDescriptionRules()
        {
            return ruleBuilder.Must(ProductValidation.IsDescriptionWithinLimit)
                .WithErrorCode(nameof(ERROR_0006))
                .WithMessage(ERROR_0006);
        }

        public IRuleBuilderOptions<T, string?> ProductNewDescriptionRules()
        {
            return ruleBuilder.NotNull()
                .WithErrorCode(nameof(ERROR_0007)).WithMessage(ERROR_0007)
                .ProductDescriptionRules();
        }
    }

    extension<T>(IRuleBuilder<T, int?> ruleBuilder)
    {
        public IRuleBuilderOptions<T, int?> PageNumberRules(int defaultPageSize,
            Func<T, int?> pageSizeSelector)
        {
            return ruleBuilder
                .Must(pageNumber => PageRules.IsPageNumberNonNegative(pageNumber ?? 0))
                .WithErrorCode(nameof(ERROR_0008))
                .WithMessage(ERROR_0008)
                .Must((request, pageNumber) =>
                    PageRules.IsOffsetWithinInt(pageNumber ?? 0, pageSizeSelector(request) ?? defaultPageSize))
                .WithErrorCode(nameof(ERROR_0010))
                .WithMessage(ERROR_0010);
        }

        public IRuleBuilderOptions<T, int?> PageSizeRules(int maxPageSize)
        {
            return ruleBuilder
                .Must(pageSize => PageRules.IsPageSizeAtLeastOne(pageSize ?? 1))
                .WithErrorCode(nameof(ERROR_0009))
                .WithMessage(ERROR_0009)
                .Must(pageSize => PageRules.IsPageSizeAtMost(pageSize ?? 1, maxPageSize))
                .WithErrorCode(nameof(ERROR_0013))
                .WithMessage(ERROR_0013(maxPageSize));
        }
    }

    extension<T>(IRuleBuilder<T, decimal> ruleBuilder)
    {
        public IRuleBuilderOptions<T, decimal> ProductPriceRules()
        {
            return ruleBuilder.Must(ProductValidation.IsPricePositive)
                .WithErrorCode(nameof(ERROR_0003))
                .WithMessage(ERROR_0003)
                .Must(ProductValidation.IsPriceWithinLimit)
                .WithErrorCode(nameof(ERROR_0011))
                .WithMessage(ERROR_0011)
                .Must(ProductValidation.HasAtMostTwoDecimalPlaces)
                .WithErrorCode(nameof(ERROR_0012))
                .WithMessage(ERROR_0012);
        }
    }
}
