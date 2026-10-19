using Microsoft.AspNetCore.Diagnostics;
using RepositoryContracts;

namespace WebAPI.ErrorHandling;

public class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> logger;

    public ApiExceptionHandler(ILogger<ApiExceptionHandler> logger)
    {
        this.logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        bool notFound = exception is EntityNotFoundException;
        int statusCode = notFound
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status500InternalServerError;

        if (!notFound)
        {
            logger.LogError(exception, "Request {TraceId} failed.", httpContext.TraceIdentifier);
        }

        await Results.Problem(
            statusCode: statusCode,
            title: notFound ? "Resource not found" : "An unexpected server error occurred.",
            detail: notFound ? exception.Message : null,
            extensions: new Dictionary<string, object?>
            {
                ["traceId"] = httpContext.TraceIdentifier
            }).ExecuteAsync(httpContext);

        return true;
    }
}
