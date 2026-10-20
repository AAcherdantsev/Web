using FastEndpoints;

namespace Web.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring endpoint summaries in a web API.
/// </summary>
public static class EndpointSummaryExtension
{
    extension(EndpointSummary summary)
    {
        /// <summary>
        /// Adds standard error response codes and their descriptions to the specified endpoint summary.
        /// This includes responses for Too Many Requests (429), Service Unavailable (503),
        /// and Internal Server Error (500).
        /// </summary>
        public void AddErrorResponses()
        {
            summary.Response(StatusCodes.Status429TooManyRequests,
                "Too many requests have been made in a short period of time.");
            summary.Response(StatusCodes.Status503ServiceUnavailable, "API is not available.");
            summary.Response(StatusCodes.Status500InternalServerError, "Internal server error.");
        }
    }
}
