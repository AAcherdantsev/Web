namespace Web.Api.Configuration.RateLimiting;

/// <summary>
///     Names of the rate-limiting policies that endpoints can require.
/// </summary>
public static class RateLimitingPolicies
{
    /// <summary>
    ///     Policy for read requests (<c>GET</c>).
    /// </summary>
    public const string READS = "reads";

    /// <summary>
    ///     Policy for writing requests (<c>POST</c>, <c>PATCH</c>).
    /// </summary>
    public const string WRITES = "writes";
}
