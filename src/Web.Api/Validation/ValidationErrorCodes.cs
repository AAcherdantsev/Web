using Web.Domain.Entities;

namespace Web.Api.Validation;

public static class ValidationErrorCodes
{
    public static readonly string ERROR_0001 = "The name must not be empty.";

    public static readonly string ERROR_0002 = $"The name must not be longer than {ProductLimits.MAX_NAME_LENGTH} characters.";
    public static readonly string ERROR_0003 = "The price must be greater than 0.";

    public static readonly string ERROR_0004 = "The image URI is not valid.";
    public static readonly string ERROR_0005 = $"The image URI must not be longer than {ProductLimits.MAX_IMG_URI_LENGTH} characters.";

    public static readonly string ERROR_0006 = $"The description must not be longer than {ProductLimits.MAX_DESCRIPTION_LENGTH} characters.";
}
