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
///     Represents an error indicating that the request breaks a rule of the input.
/// </summary>
public record ValidationError : Error
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ValidationError" /> class.
    /// </summary>
    /// <param name="message">The message that describes the broken rule.</param>
    public ValidationError(string message)
    {
        Message = message;
    }
}

/// <summary>
///     Represents an error indicating that the change conflicts with another change saved at the same time.
/// </summary>
public record ConflictError : Error
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ConflictError" /> class.
    /// </summary>
    /// <param name="message">The message.</param>
    public ConflictError(string message)
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
