using FluentValidation;
using Microsoft.Extensions.Options;
using Web.Api.Configuration;
using Web.Domain.Entities;
using static Web.Api.Validation.ValidationErrorCodes;

namespace Web.Api.Validation;

public static class ValidationExtensions
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> ProductNameRules()
        {
            return ruleBuilder.NotEmpty().WithErrorCode(nameof(ERROR_0001)).WithMessage(ERROR_0001)
                .MaximumLength(ProductLimits.MAX_NAME_LENGTH).WithErrorCode(nameof(ERROR_0002))
                .WithMessage(ERROR_0002);
        }

        public IRuleBuilderOptions<T, string> ProductImageUrlRules()
        {
            return ruleBuilder.Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithErrorCode(nameof(ERROR_0004))
                .WithMessage(ERROR_0004)
                .MaximumLength(ProductLimits.MAX_IMG_URI_LENGTH)
                .WithErrorCode(nameof(ERROR_0005))
                .WithMessage(ERROR_0005);
        }

        public IRuleBuilderOptions<T, string> ProductDescriptionRules()
        {
            return ruleBuilder.MaximumLength(ProductLimits.MAX_DESCRIPTION_LENGTH)
                .WithErrorCode(nameof(ERROR_0006))
                .WithMessage(ERROR_0006);
        }

        public IRuleBuilderOptions<T, string> ProductNewDescriptionRules()
        {
            return ruleBuilder.NotNull()
                .WithErrorCode(nameof(ERROR_0007)).WithMessage(ERROR_0007)
                .ProductDescriptionRules();
        }
    }

    extension<T>(IRuleBuilder<T, int> ruleBuilder)
    {
        public IRuleBuilderOptions<T, int> PageNumberRules()
        {
            return ruleBuilder.GreaterThanOrEqualTo(0)
                .WithErrorCode(nameof(ERROR_0008))
                .WithMessage(ERROR_0008);
        }
    }

    extension<T>(IRuleBuilder<T, int?> ruleBuilder)
    {
        public IRuleBuilderOptions<T, int?> PageSizeRules(IOptions<PaginationOptions> pagination)
        {
            return ruleBuilder.InclusiveBetween(1, pagination.Value.MaxPageSize)
                .WithErrorCode(nameof(ERROR_0009))
                .WithMessage(ERROR_0009(pagination.Value.MaxPageSize));
        }
    }

    extension<T>(IRuleBuilder<T, decimal> ruleBuilder)
    {
        public IRuleBuilderOptions<T, decimal> ProductPriceRules()
        {
            return ruleBuilder.GreaterThan(0)
                .WithErrorCode(nameof(ERROR_0003))
                .WithMessage(ERROR_0003);
        }
    }
}
