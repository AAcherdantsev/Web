using FluentValidation;
using Web.Domain.Entities;
using static Web.Api.Validation.ValidationErrorCodes;

namespace Web.Api.Validation;

public static class ValidationExtensions
{
    extension<T>(IRuleBuilder<T, string> ruleBuilder)
    {
        public IRuleBuilderOptions<T, string> ProductNameRules()
        {
            return ruleBuilder.NotEmpty().WithErrorCode(nameof(ERROR_0001)).WithMessage(_ => ERROR_0001)
                .MaximumLength(ProductLimits.MAX_NAME_LENGTH).WithErrorCode(nameof(ERROR_0002))
                .WithMessage(_ => ERROR_0002);
        }

        public IRuleBuilderOptions<T, string> ProductImageUrlRules()
        {
            return ruleBuilder.Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithErrorCode(nameof(ERROR_0004))
                .WithMessage(_ => ERROR_0004)
                .MaximumLength(ProductLimits.MAX_IMG_URI_LENGTH)
                .WithErrorCode(nameof(ERROR_0005))
                .WithMessage(_ => ERROR_0005);
        }

        public IRuleBuilderOptions<T, string> ProductDescriptionRules()
        {
            return ruleBuilder.MaximumLength(ProductLimits.MAX_DESCRIPTION_LENGTH)
                .WithErrorCode(nameof(ERROR_0006))
                .WithMessage(_ => ERROR_0006);
        }
    }

    extension<T>(IRuleBuilder<T, int> ruleBuilder)
    {
        public IRuleBuilderOptions<T, int> ProductPriceRules()
        {
            return ruleBuilder.GreaterThan(0).WithErrorCode(nameof(ERROR_0003)).WithMessage(_ => ERROR_0003);
        }
    }
}
