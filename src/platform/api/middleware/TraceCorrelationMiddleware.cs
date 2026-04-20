using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Whycespace.Platform.Api.Middleware;

/// <summary>
/// R5.A / R-TRACE-CORRELATION-BRIDGE-01 — bridges the existing
/// <see cref="CorrelationIdMiddleware"/> correlation id with the ambient
/// OpenTelemetry <see cref="Activity"/>:
///
/// <list type="bullet">
///   <item>On inbound request, tags the current <see cref="Activity"/>
///         (produced by the AspNetCore auto-instrumentation) with
///         <c>whyce.correlation_id</c> so Jaeger / Tempo can filter and
///         join traces to the application-layer correlation set.</item>
///   <item>On response start, echoes the current trace id back on a
///         <c>traceresponse</c> header (W3C-style) so the caller can pivot
///         directly into the trace backend.</item>
/// </list>
///
/// <para>Runs AFTER <see cref="CorrelationIdMiddleware"/> so the correlation
/// id is already on <see cref="HttpContext.Items"/>, and AFTER
/// <c>UseRouting()</c> so the AspNetCore instrumentation has started its
/// root span.</para>
/// </summary>
public sealed class TraceCorrelationMiddleware
{
    public const string TraceResponseHeader = "traceresponse";

    // R5.A / R-TRACE-SOURCE-VOCABULARY-01 — mirrors
    // Whycespace.Runtime.Observability.WhyceActivitySources.Attributes.CorrelationId.
    // Platform_api does NOT reference the runtime layer (DG-R5-01), so we
    // inline the canonical string here. The runtime-layer source of truth
    // is the authoritative declaration; the validator test
    // `Trace_correlation_middleware_uses_canonical_whyce_correlation_id_attribute_key`
    // pins drift between the two.
    private const string WhyceCorrelationIdAttribute = "whyce.correlation_id";

    private readonly RequestDelegate _next;

    public TraceCorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var activity = Activity.Current;
        if (activity is not null &&
            context.Items.TryGetValue(CorrelationIdMiddleware.ItemsKey, out var raw) &&
            raw is Guid correlationId)
        {
            activity.SetTag(WhyceCorrelationIdAttribute, correlationId);
        }

        context.Response.OnStarting(() =>
        {
            var current = Activity.Current;
            if (current is not null && !context.Response.Headers.ContainsKey(TraceResponseHeader))
            {
                // W3C trace-context traceparent shape:
                //   {version}-{trace-id}-{parent-id}-{trace-flags}
                // Callers use this to pivot directly into Jaeger / Tempo.
                context.Response.Headers[TraceResponseHeader] =
                    $"00-{current.TraceId}-{current.SpanId}-{(byte)current.ActivityTraceFlags:x2}";
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
