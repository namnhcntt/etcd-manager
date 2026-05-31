using System.Net;
using System.Text.Json;

namespace EtcdManager.API.Core.Exceptions;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var code = HttpStatusCode.InternalServerError;
        string message;
        if (exception is DomainListException)
        {
            message = string.Empty;
            code = HttpStatusCode.BadRequest;
            var domainListException = (exception as DomainListException)!;
            foreach (var error in domainListException.Errors)
            {
                message += $"{error.Key.message}{Environment.NewLine}";
            }
        }
        else
        {
            // Do not expose internal exception details to the client
            message = "An unexpected error occurred. Please try again later.";
        }
        var result = JsonSerializer.Serialize(new { error = message });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        return context.Response.WriteAsync(result);
    }
}
