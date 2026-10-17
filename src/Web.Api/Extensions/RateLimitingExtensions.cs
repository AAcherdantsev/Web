using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using Web.Api.Configuration.RateLimiting;

namespace Web.Api.Extensions;

/// <summary>
///     Registers per-client rate limiting for the API.
/// </summary>
/// <remarks>
///     Each policy uses a token bucket per client IP address. Requests over the limit are rejected immediately
///     with <c>429 Too Many Requests</c>, a <c>Retry-After</c> header, and a problem details body.
///     Rejected requests are not queued.
/// </remarks>
public static class RateLimitingExtensions
{
    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        ///     Adds the rate limiter, its validated settings, and the read and write policies.
        /// </summary>
        public void AddApiRateLimiting()
        {
            builder.Services.AddOptions<RateLimitingOptions>()
                .Bind(builder.Configuration.GetSection(RateLimitingOptions.SECTION_NAME))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy(RateLimitingPolicies.READS, context =>
                    CreatePartition(context, settings => settings.Reads));

                options.AddPolicy(RateLimitingPolicies.WRITES, context =>
                    CreatePartition(context, settings => settings.Writes));

                options.OnRejected = async (context, cancellationToken) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                    {
                        int seconds = Math.Max(1, (int) Math.Ceiling(retryAfter.TotalSeconds));
                        context.HttpContext.Response.Headers.RetryAfter =
                            seconds.ToString(CultureInfo.InvariantCulture);
                    }

                    await TypedResults.Problem(
                        title: "Too many requests.",
                        detail:
                        "The request limit for this client has been exceeded. Retry after the time in the Retry-After header.",
                        statusCode: StatusCodes.Status429TooManyRequests).ExecuteAsync(context.HttpContext);
                };
            });
        }

        /// <summary>
        ///     Creates the rate limiter partition for one request, keyed by the client IP address.
        /// </summary>
        private static RateLimitPartition<string> CreatePartition(
            HttpContext context,
            Func<RateLimitingOptions, TokenBucketOptions> select)
        {
            TokenBucketOptions settings =
                select(context.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value);
            string clientKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetTokenBucketLimiter(clientKey,
                _ => new()
                {
                    TokenLimit = settings.TokenLimit,
                    TokensPerPeriod = settings.TokensPerPeriod,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(settings.ReplenishmentPeriodSeconds),
                    AutoReplenishment = true,
                    QueueLimit = 0
                });
        }
    }
}
