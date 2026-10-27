namespace KivaBelion.API.Common;

/// <summary>
/// Gives each request an id, adds it to the response header X-Correlation-Id and to every log line,
/// so a problem report can be matched to the exact log entries.
/// </summary>
public sealed class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var id = context.Request.Headers[HeaderName].FirstOrDefault();

        // Accept a caller's id only if it is short and made of safe characters.
        if (string.IsNullOrWhiteSpace(id) || id.Length > 64 || !id.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'))
        {
            id = Guid.NewGuid().ToString("N");
        }

        context.Response.Headers[HeaderName] = id;

        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = id }))
        {
            await next(context);
        }
    }
}
