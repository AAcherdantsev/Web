using FluentResults;

namespace Web.Application.Common.Errors;

/// <summary>
///     Represents an error indicating that a specified resource was not found.
/// </summary>
public record NotFoundError : Error
{
    /// <summary>
    ///     Creates a new instance of the <see cref="NotFoundError" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public NotFoundError(string message)
    {
        Message = message;
    }
}

/// <summary>
///     Provides a base class for representing errors.
/// </summary>
public abstract record Error : IError
{
    /// <inheritdoc />
    public string Message { get; init; } = string.Empty;

    /// <inheritdoc />
    public Dictionary<string, object> Metadata { get; init; } = [];

    /// <inheritdoc />
    public List<IError> Reasons { get; init; } = [];
}
