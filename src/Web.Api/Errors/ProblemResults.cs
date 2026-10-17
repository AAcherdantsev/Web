using FluentResults;

namespace Web.Api.Errors;

/// <summary>
///     Builds RFC 7807 problem responses for the endpoints.
/// </summary>
public static class ProblemResults
{
    /// <summary>
    ///     Creates a <c>404 Not Found</c> problem response.
    /// </summary>
    /// <param name="detail">A human-readable explanation of the failure.</param>
    public static IResult NotFound(string detail)
    {
        return Results.Problem(title: "Resource not found.", detail: detail, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    ///     Creates a <c>400 Bad Request</c> problem response from the failed result errors.
    /// </summary>
    /// <param name="errors">The errors reported by the repository.</param>
    public static IResult BadRequest(IEnumerable<IError> errors)
    {
        return Results.Problem(title: "The request is invalid.",
            detail: string.Join(" ", errors.Select(e => e.Message)),
            statusCode: StatusCodes.Status400BadRequest);
    }
}
