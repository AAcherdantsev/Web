using FluentResults;
using Web.Application.Common.Errors;

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
    ///     The problem type of <c>409 Conflict</c> responses.
    /// </summary>
    public const string CONFLICT_TYPE = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.10";

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
    ///     Creates a <c>409 Conflict</c> problem response, for a change that was made on top of another change.
    /// </summary>
    /// <param name="detail">A human-readable explanation of the failure.</param>
    public static IResult Conflict(string detail)
    {
        return TypedResults.Problem(
            type: CONFLICT_TYPE,
            title: "The request conflicts with the current state of the resource.",
            detail: detail,
            statusCode: StatusCodes.Status409Conflict);
    }

    /// <summary>
    ///     Creates the problem response for a failed result. A <see cref="NotFoundError" /> gives <c>404</c>, a
    ///     <see cref="ConflictError" /> gives <c>409</c>, and anything else is a validation failure, which gives <c>400</c>.
    /// </summary>
    /// <param name="errors">The errors of the failed result.</param>
    public static IResult ForErrors(IEnumerable<IError> errors)
    {
        List<IError> list = errors.ToList();

        if (list.OfType<NotFoundError>().FirstOrDefault() is { } notFound)
        {
            return NotFound(notFound.Message);
        }

        if (list.OfType<ConflictError>().FirstOrDefault() is { } conflict)
        {
            return Conflict(conflict.Message);
        }

        return BadRequest(list);
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
