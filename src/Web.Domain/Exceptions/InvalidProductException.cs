namespace Web.Domain.Exceptions;

/// <summary>
///     Thrown when a product would break a business rule, for example when its price is negative.
/// </summary>
public sealed class InvalidProductException : Exception
{
    /// <summary>
    ///     Creates a new instance of the <see cref="InvalidProductException" /> class.
    /// </summary>
    /// <param name="violations">The messages of the broken rules.</param>
    public InvalidProductException(IReadOnlyList<string> violations)
        : base(string.Join(" ", violations))
    {
        Violations = violations;
    }

    /// <summary>
    ///     The messages of all the broken rules.
    /// </summary>
    public IReadOnlyList<string> Violations { get; }
}
