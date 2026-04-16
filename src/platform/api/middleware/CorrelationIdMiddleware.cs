using Microsoft.AspNetCore.Http;

namespace Whycespace.Platform.Api.Middleware;

/// <summary>
/// Closed-loop correlation: ensures every HTTP request carries an
/// <c>X-Correlation-Id</c> header on its way IN and OUT.
///
/// Inbound: if the request has <c>X-Correlation-Id</c> as a parseable Guid,
/// it's stored on <see cref="HttpContext.Items"/> under <see cref="ItemsKey"/>.
/// Otherwise a fresh Guid is generated. Either way the value is echoed back
/// on the response under the same header.
///
/// Controllers retrieve the current request's id via the
/// <see cref="ControllerCorrelationExtensions.RequestCorrelationId"/> helper,
/// which is what <c>ApiResponse.Ok/Fail</c> uses to populate
/// <c>meta.correlationId</c>. Together they close POST → GET continuity.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemsKey = "whyce.correlation_id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = ExtractInbound(context) ?? Guid.NewGuid();
        context.Items[ItemsKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(HeaderName))
                context.Response.Headers[HeaderName] = correlationId.ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private static Guid? ExtractInbound(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var values))
            return null;
        var raw = values.ToString();
        return Guid.TryParse(raw, out var parsed) ? parsed : null;
    }
}
