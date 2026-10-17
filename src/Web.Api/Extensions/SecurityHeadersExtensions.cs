using Microsoft.Net.Http.Headers;

namespace Web.Api.Extensions;

/// <summary>
///     Adds security headers to every response.
/// </summary>
public static class SecurityHeadersExtensions
{
    extension(IApplicationBuilder app)
    {
        /// <summary>
        ///     Adds the security headers middleware to the request pipeline.
        /// </summary>
        /// <remarks>
        ///     The headers are set when the response starts, not before the rest of the pipeline runs. The exception
        ///     handler clears the response when it handles an error, which would otherwise remove them from error responses.
        ///     The Content-Security-Policy header is not sent in Development, so the Swagger UI can load its scripts.
        /// </remarks>
        /// <param name="isDevelopment">Whether the application runs in the Development environment.</param>
        public IApplicationBuilder UseSecurityHeaders(bool isDevelopment)
        {
            return app.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    IHeaderDictionary headers = context.Response.Headers;
                    headers[HeaderNames.XContentTypeOptions] = "nosniff";
                    headers[HeaderNames.XFrameOptions] = "DENY";
                    headers["Referrer-Policy"] = "no-referrer";

                    if (!isDevelopment)
                    {
                        headers[HeaderNames.ContentSecurityPolicy] = "default-src 'none'; frame-ancestors 'none'";
                    }

                    return Task.CompletedTask;
                });

                await next(context);
            });
        }
    }
}
