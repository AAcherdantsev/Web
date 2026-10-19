using System.Globalization;
using Web.Domain.Entities;

namespace Web.Api.Validation;

public static class ValidationErrorCodes
{
    public static readonly string ERROR_0001 = "The name must not be empty.";

    public static readonly string ERROR_0002 =
        $"The name must not be longer than {ProductLimits.MAX_NAME_LENGTH} characters.";

    public static readonly string ERROR_0003 = "The price must be greater than 0.";

    public static readonly string ERROR_0004 = "The image URI is not valid.";

    public static readonly string ERROR_0005 =
        $"The image URI must not be longer than {ProductLimits.MAX_IMG_URI_LENGTH} characters.";

    public static readonly string ERROR_0006 =
        $"The description must not be longer than {ProductLimits.MAX_DESCRIPTION_LENGTH} characters.";

    public static readonly string ERROR_0007 = "The description is required.";

    public static readonly string ERROR_0008 = "The page number must be greater than or equal to 0.";
    public static readonly string ERROR_0010 = "The requested page is beyond the supported range.";

    public static readonly string ERROR_0011 =
        $"The price must be at most {ProductPrice.MAX.ToString(CultureInfo.InvariantCulture)} and have at most 2 decimal places.";

    public static string ERROR_0009(int maxPageSize)
    {
        return $"The page size must be between 1 and {maxPageSize}.";
    }
}
