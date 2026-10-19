using FluentValidation;
using Web.Domain.Entities;
using static Web.Application.Products.ValidationErrorCodes;

namespace Web.Application.Products;

public static class ProductValidationRules
{
    extension<T>(IRuleBuilder<T, string?> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string?> ProductNameRules()
        {
            return ruleBuilder.NotEmpty().WithErrorCode(nameof(ERROR_0001)).WithMessage(ERROR_0001)
                .MaximumLength(ProductLimits.MAX_NAME_LENGTH).WithErrorCode(nameof(ERROR_0002))
                .WithMessage(ERROR_0002);
        }

        public IRuleBuilderOptions<T, string?> ProductImageUrlRules()
        {
            return ruleBuilder.Must(ProductImageUri.IsValid)
                .WithErrorCode(nameof(ERROR_0004))
                .WithMessage(ERROR_0004)
                .MaximumLength(ProductLimits.MAX_IMG_URI_LENGTH)
                .WithErrorCode(nameof(ERROR_0005))
                .WithMessage(ERROR_0005);
        }

        public IRuleBuilderOptions<T, string?> ProductDescriptionRules()
        {
            return ruleBuilder.MaximumLength(ProductLimits.MAX_DESCRIPTION_LENGTH)
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
                .GreaterThanOrEqualTo(0)
                .WithErrorCode(nameof(ERROR_0008))
                .WithMessage(ERROR_0008)
                .Must((request, pageNumber) =>
                    (long) (pageNumber ?? 0) *
                    (pageSizeSelector(request) ?? defaultPageSize)
                    <= int.MaxValue)
                .WithErrorCode(nameof(ERROR_0010))
                .WithMessage(ERROR_0010);
        }
    }

    extension<T>(IRuleBuilder<T, int?> ruleBuilder)
    {
        public IRuleBuilderOptions<T, int?> PageSizeRules(int maxPageSize)
        {
            return ruleBuilder.InclusiveBetween(1, maxPageSize)
                .WithErrorCode(nameof(ERROR_0009))
                .WithMessage(ERROR_0009(maxPageSize));
        }
    }

    extension<T>(IRuleBuilder<T, decimal> ruleBuilder)
    {
        public IRuleBuilderOptions<T, decimal> ProductPriceRules()
        {
            return ruleBuilder.GreaterThan(0)
                .WithErrorCode(nameof(ERROR_0003))
                .WithMessage(ERROR_0003)
                .LessThanOrEqualTo(ProductPrice.MAX)
                .WithErrorCode(nameof(ERROR_0011))
                .WithMessage(ERROR_0011)
                .Must(ProductPrice.HasAtMostTwoDecimalPlaces)
                .WithErrorCode(nameof(ERROR_0012))
                .WithMessage(ERROR_0012);
        }
    }
}
