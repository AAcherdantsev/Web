using FastEndpoints;

namespace Web.Api.Extensions;

public static class EndpointSummaryExtension
{
    extension(EndpointSummary summary)
    {
        public void AddErrorResponses()
        {
            summary.Response(StatusCodes.Status429TooManyRequests,
                "Too many requests have been made in a short period of time.");
            summary.Response(StatusCodes.Status503ServiceUnavailable, "API is not available.");
            summary.Response(StatusCodes.Status500InternalServerError, "Internal server error.");
        }
    }
}
