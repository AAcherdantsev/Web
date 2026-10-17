using Microsoft.AspNetCore.Diagnostics;

namespace Web.Api.Extensions;

/// <summary>
///     Provides a mechanism to handle exceptions globally in an ASP.NET Core application.
/// </summary>
/// <remarks>
///     Every unhandled exception is turned into an RFC 7807 problem details response with status 500
///     (<c>application/problem+json</c>). Exception details are only exposed in the Development environment.
///     The exception itself is logged by the ASP.NET Core exception handler middleware.
/// </remarks>
public static class ExceptionHandler
{
    private const string STACK_TRACE = "stackTrace";
    private const string ERROR_MESSAGE = "An unexpected error occurred.";

    extension(WebApplication app)
    {
        /// <summary>
        ///     Configures a custom global exception handler for the application.
        /// </summary>
        public void UseCustomExceptionHandler()
        {
            bool isDev = app.Environment.IsDevelopment();

            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    IExceptionHandlerFeature? exceptionHandlerFeature =
                        context.Features.Get<IExceptionHandlerFeature>();

                    if (exceptionHandlerFeature == null)
                    {
                        return;
                    }

                    Exception ex = exceptionHandlerFeature.Error;

                    Dictionary<string, object?>? extensions = isDev
                        ? new Dictionary<string, object?> { [STACK_TRACE] = ex.StackTrace }
                        : null;

                    await TypedResults.Problem(
                        title: ERROR_MESSAGE,
                        detail: isDev ? ex.Message : null,
                        statusCode: StatusCodes.Status500InternalServerError,
                        extensions: extensions).ExecuteAsync(context);
                });
            });
        }
    }
}
