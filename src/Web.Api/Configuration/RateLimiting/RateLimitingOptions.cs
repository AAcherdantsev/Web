using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Web.Api.Configuration.RateLimiting;

/// <summary>
///     Rate limiting settings. Bound from the <c>RateLimiting</c> configuration section.
/// </summary>
public class RateLimitingOptions
{
    /// <summary>
    ///     The configuration section name.
    /// </summary>
    public const string SECTION_NAME = "RateLimiting";

    /// <summary>
    ///     Limits for read requests (<c>GET</c>).
    /// </summary>
    [ValidateObjectMembers]
    public TokenBucketOptions Reads { get; set; } =
        new() { TokenLimit = 100, TokensPerPeriod = 100, ReplenishmentPeriodSeconds = 60 };

    /// <summary>
    ///     Limits for write requests (<c>POST</c>, <c>PATCH</c>).
    /// </summary>
    [ValidateObjectMembers]
    public TokenBucketOptions Writes { get; set; } =
        new() { TokenLimit = 20, TokensPerPeriod = 20, ReplenishmentPeriodSeconds = 60 };
}

/// <summary>
///     Settings for one token bucket. The bucket holds up to <see cref="TokenLimit" /> tokens (the allowed burst)
///     and refills <see cref="TokensPerPeriod" /> tokens every <see cref="ReplenishmentPeriodSeconds" /> seconds.
/// </summary>
public class TokenBucketOptions
{
    /// <summary>
    ///     The maximum number of tokens in the bucket, which is also the largest burst a client can send at once.
    /// </summary>
    [Range(1, 10_000)]
    public int TokenLimit { get; set; }

    /// <summary>
    ///     The number of tokens added on each refill.
    /// </summary>
    [Range(1, 10_000)]
    public int TokensPerPeriod { get; set; }

    /// <summary>
    ///     The time between refills, in seconds.
    /// </summary>
    [Range(1, 3600)]
    public int ReplenishmentPeriodSeconds { get; set; }
}
