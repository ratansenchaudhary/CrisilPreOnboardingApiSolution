using System.Diagnostics;
using System.Text;

namespace CrisilPreOnboardingApi.Infrastructure;

public sealed class RequestResponseLoggingMiddleware : IMiddleware
{
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var sw = Stopwatch.StartNew();

        // Request body (best-effort, truncated)
        string? requestBody = null;
        if (context.Request.ContentLength is > 0 &&
            context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (requestBody.Length > 20000) requestBody = requestBody[..20000] + "...(truncated)";
        }

        // Response body capture
        var originalBodyStream = context.Response.Body;
        await using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await next(context);
        }
        finally
        {
            sw.Stop();

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            if (responseText.Length > 20000) responseText = responseText[..20000] + "...(truncated)";

            await responseBodyStream.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;

            _logger.LogInformation(
                "HTTP {Method} {Path} => {StatusCode} in {ElapsedMs}ms | TraceId={TraceId} | Req={Req} | Res={Res}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                context.TraceIdentifier,
                requestBody ?? "",
                responseText ?? ""
            );
        }
    }
}
