using Web.Application.Common;
using Web.Application.Products;

namespace Web.Api.Validation;

/// <summary>
///     The codes and messages of the request validation errors. The codes are part of the API. The messages of the
///     product and page rules come from the application layer, so the HTTP layer reports the same text as the domain.
/// </summary>
public static class ValidationErrorCodes
{
    public static readonly string ERROR_0001 = ProductValidation.NAME_EMPTY;

    public static readonly string ERROR_0002 = ProductValidation.NAME_TOO_LONG;

    public static readonly string ERROR_0003 = ProductValidation.PRICE_NOT_POSITIVE;

    public static readonly string ERROR_0004 = ProductValidation.IMG_URI_INVALID;

    public static readonly string ERROR_0005 = ProductValidation.IMG_URI_TOO_LONG;

    public static readonly string ERROR_0006 = ProductValidation.DESCRIPTION_TOO_LONG;

    public static readonly string ERROR_0007 = "The description is required.";

    public static readonly string ERROR_0008 = PageRules.PAGE_NUMBER_NEGATIVE;

    public static readonly string ERROR_0009 = PageRules.PAGE_SIZE_TOO_SMALL;

    public static readonly string ERROR_0010 = PageRules.PAGE_OFFSET_TOO_LARGE;

    public static readonly string ERROR_0011 = ProductValidation.PRICE_TOO_LARGE;

    public static readonly string ERROR_0012 = ProductValidation.PRICE_TOO_MANY_DECIMALS;

    public static string ERROR_0013(int maxPageSize)
    {
        return PageRules.PageSizeTooLarge(maxPageSize);
    }
}
