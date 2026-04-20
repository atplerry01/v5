using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Whycespace.Platform.Api.Middleware;

/// <summary>
/// R5.A Phase 2 / R-TRACE-LOG-CORRELATION-01 — wraps every request in an
/// <see cref="ILogger"/> scope carrying the canonical correlation dimensions
/// (<c>trace_id</c>, <c>span_id</c>, <c>correlation_id</c>, <c>tenant_id</c>).
/// Every log line emitted downstream inherits these fields so operators can
/// pivot between logs, traces, and metrics on a single identifier set.
///
/// <para>Ordering contract: registered AFTER <see cref="CorrelationIdMiddleware"/>
/// and <see cref="TraceCorrelationMiddleware"/> so the correlation id is on
/// the items bag and the AspNetCore auto-instrumentation root Activity is
/// already active. The tenant id is read from the <c>X-Tenant-Id</c> header
/// (advisory, per the existing intake partitioning convention); absent = the
/// scope simply omits the tenant key rather than inventing a default.</para>
///
/// <para>Scope keys match the canonical span attribute vocabulary
/// (<c>whyce.correlation_id</c>) using the dot-to-underscore convention
/// standard logging frameworks apply. Seq / Grafana Loki / Kibana pick these
/// up as structured fields automatically.</para>
/// </summary>
public sealed class LogCorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogCorrelationMiddleware> _logger;

    public LogCorrelationMiddleware(RequestDelegate next, ILogger<LogCorrelationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var scope = new Dictionary<string, object?>(capacity: 4);

        var activity = Activity.Current;
        if (activity is not null)
        {
            scope["trace_id"] = activity.TraceId.ToString();
            scope["span_id"] = activity.SpanId.ToString();
        }

        if (context.Items.TryGetValue(CorrelationIdMiddleware.ItemsKey, out var raw)
            && raw is Guid correlationId)
        {
            scope["correlation_id"] = correlationId;
        }

        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantValues))
        {
            var tenantId = tenantValues.ToString();
            if (!string.IsNullOrWhiteSpace(tenantId))
                scope["tenant_id"] = tenantId;
        }

        using (_logger.BeginScope(scope))
        {
            await _next(context);
        }
    }
}
