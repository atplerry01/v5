using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Prometheus;

namespace Whycespace.Platform.Host.Observability;

/// <summary>
/// HTTP-level metrics middleware for Prometheus. Tracks request duration, count,
/// and errors at the HTTP boundary. Lives in the host so the <c>prometheus-net</c>
/// dependency stays on the infra side per R-TRACE-LAYER-DISCIPLINE-01.
/// The runtime pipeline <c>MetricsMiddleware</c> continues to cover command
/// execution inside the control plane via <c>System.Diagnostics.Metrics</c>.
/// </summary>
public sealed class HttpMetricsMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly Histogram RequestDuration = Metrics.CreateHistogram(
        "request_duration_seconds",
        "Duration of HTTP requests in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "method", "endpoint", "status_code" },
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 16)
        });

    private static readonly Counter RequestCount = Metrics.CreateCounter(
        "request_count_total",
        "Total number of HTTP requests",
        new CounterConfiguration
        {
            LabelNames = new[] { "method", "endpoint", "status_code" }
        });

    private static readonly Counter ErrorCount = Metrics.CreateCounter(
        "error_count_total",
        "Total number of HTTP errors",
        new CounterConfiguration
        {
            LabelNames = new[] { "method", "endpoint" }
        });

    public HttpMetricsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var method = context.Request.Method;
        var endpoint = context.Request.Path.Value ?? "/";
        var sw = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        catch
        {
            ErrorCount.WithLabels(method, endpoint).Inc();
            throw;
        }
        finally
        {
            sw.Stop();
            var statusCode = context.Response.StatusCode.ToString();
            RequestDuration.WithLabels(method, endpoint, statusCode).Observe(sw.Elapsed.TotalSeconds);
            RequestCount.WithLabels(method, endpoint, statusCode).Inc();

            if (context.Response.StatusCode >= 500)
            {
                ErrorCount.WithLabels(method, endpoint).Inc();
            }
        }
    }
}
