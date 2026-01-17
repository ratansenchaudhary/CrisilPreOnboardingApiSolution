using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using CrisilPreOnboardingApi.Models;

namespace CrisilPreOnboardingApi.Infrastructure;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception. TraceId={TraceId}", traceId);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = new ApiErrorResponse
            {
                Code = "INTERNAL_ERROR",
                Message = "An unexpected error occurred.",
                Errors = new()
                {
                    new ApiFieldError { Field = "", ErrorCode = "INTERNAL_ERROR", Message = "Please contact support with the traceId." }
                },
                TraceId = traceId
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }));
        }
    }
}
