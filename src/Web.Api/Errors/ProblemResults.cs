using FluentResults;

namespace Web.Api.Errors;

/// <summary>
///     Builds the problem details responses for expected failures. Every response has the same members, so that
///     clients handle them the same way.
/// </summary>
public static class ProblemResults
{
    /// <summary>
    ///     The problem type of <c>404 Not Found</c> responses.
    /// </summary>
    public const string NOT_FOUND_TYPE = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.5";

    /// <summary>
    ///     The problem type of <c>400 Bad Request</c> responses.
    /// </summary>
    public const string BAD_REQUEST_TYPE = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1";

    /// <summary>
    ///     Creates a <c>404 Not Found</c> problem response.
    /// </summary>
    /// <param name="detail">A human-readable explanation of the failure.</param>
    public static IResult NotFound(string detail)
    {
        return TypedResults.Problem(
            type: NOT_FOUND_TYPE,
            title: "Resource not found.",
            detail: detail,
            statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    ///     Creates a <c>400 Bad Request</c> problem response from the failed result errors.
    /// </summary>
    /// <param name="errors">The errors reported by the application services.</param>
    public static IResult BadRequest(IEnumerable<IError> errors)
    {
        return TypedResults.Problem(
            type: BAD_REQUEST_TYPE,
            title: "The request is invalid.",
            detail: string.Join(" ", errors.Select(e => e.Message)),
            statusCode: StatusCodes.Status400BadRequest);
    }
}
