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
        /// <summary>
        /// Defines validation rules for product names.
        /// Ensures that the name of a product is provided and within defined length limits.
        /// </summary>
        /// <returns>
        /// A rule builder option that validates the product name,
        /// returning error codes and messages if the name is:
        /// - Not provided (ERROR_0001)
        /// - Exceeds the maximum allowable length (ERROR_0002)
        /// </returns>
        public IRuleBuilderOptions<T, string?> ProductNameRules()
        {
            return ruleBuilder.Must(ProductValidation.IsNameProvided).WithErrorCode(nameof(ERROR_0001))
                .WithMessage(ERROR_0001)
                .Must(ProductValidation.IsNameWithinLimit).WithErrorCode(nameof(ERROR_0002))
                .WithMessage(ERROR_0002);
        }

        /// <summary>
        /// Defines validation rules for product image URLs.
        /// Ensures that the image URL provided is valid and adheres to the required length constraints.
        /// </summary>
        /// <returns>
        /// A rule builder option that validates the product image URL,
        /// returning error codes and messages if the URL is:
        /// - Invalid (ERROR_0004)
        /// - Exceeds the maximum allowable length (ERROR_0005)
        /// </returns>
        public IRuleBuilderOptions<T, string?> ProductImageUrlRules()
        {
            return ruleBuilder.Must(ProductValidation.IsImgUriValid)
                .WithErrorCode(nameof(ERROR_0004))
                .WithMessage(ERROR_0004)
                .Must(ProductValidation.IsImgUriWithinLimit)
                .WithErrorCode(nameof(ERROR_0005))
                .WithMessage(ERROR_0005);
        }

        /// <summary>
        /// Defines validation rules for product descriptions.
        /// Ensures that the description of a product adheres to the defined character limit.
        /// </summary>
        /// <returns>
        /// A rule builder option that validates the product description,
        /// returning an error code and message if the description length exceeds the allowable limit (ERROR_0006).
        /// </returns>
        public IRuleBuilderOptions<T, string?> ProductDescriptionRules()
        {
            return ruleBuilder.Must(ProductValidation.IsDescriptionWithinLimit)
                .WithErrorCode(nameof(ERROR_0006))
                .WithMessage(ERROR_0006);
        }

        /// <summary>
        /// Defines validation rules for new product descriptions.
        /// Ensures that the description is provided and adheres to the defined character limit.
        /// </summary>
        /// <returns>
        /// A rule builder option that validates the new product description,
        /// returning error codes and messages if:
        /// - The description is not provided (ERROR_0007).
        /// - The description length exceeds the allowable limit (ERROR_0006).
        /// </returns>
        public IRuleBuilderOptions<T, string?> ProductNewDescriptionRules()
        {
            return ruleBuilder.NotNull()
                .WithErrorCode(nameof(ERROR_0007)).WithMessage(ERROR_0007)
                .ProductDescriptionRules();
        }
    }

    extension<T>(IRuleBuilder<T, int?> ruleBuilder)
    {
        /// <summary>
        /// Defines validation rules for page numbers in pagination.
        /// Ensures that the page number is non-negative and that the calculated page offset does not exceed the allowable range for integer values.
        /// </summary>
        /// <param name="defaultPageSize">The default page size to be used if no specific size is provided.</param>
        /// <param name="pageSizeSelector">A function that retrieves the page size from a specific context or object.</param>
        /// <returns>
        /// A rule builder option that validates page numbers, returning error codes and messages if:
        /// - The page number is negative (ERROR_0008).
        /// - The page offset exceeds the maximum allowable range (ERROR_0010).
        /// </returns>
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

        /// <summary>
        /// Defines validation rules for the page size.
        /// Ensures the page size is within the valid range, checking against a minimum value of 1
        /// and a specified maximum allowable size.
        /// </summary>
        /// <param name="maxPageSize">
        /// The maximum allowable page size. Requests with a page size exceeding this value will be flagged as invalid.
        /// </param>
        /// <returns>
        /// A rule builder option that validates the page size, returning error codes and messages if:
        /// - The page size is less than 1 (ERROR_0009).
        /// - The page size exceeds the specified maximum allowable size (ERROR_0013).
        /// </returns>
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
        /// <summary>
        /// Defines validation rules for product prices.
        /// Ensures that the price of a product is provided, positive, within allowable limits,
        /// and conforms to the valid format with at most two decimal places.
        /// </summary>
        /// <returns>
        /// A rule builder option that validates the product price, returning error codes
        /// and messages in the following scenarios:
        /// - If the price is not positive (ERROR_0003)
        /// - If the price exceeds the maximum allowable value (ERROR_0011)
        /// - If the price contains more than two decimal places (ERROR_0012)
        /// </returns>
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
