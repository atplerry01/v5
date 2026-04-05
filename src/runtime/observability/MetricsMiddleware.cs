using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Prometheus;

namespace Whyce.Runtime.Observability;

public sealed class MetricsMiddleware
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

    private static readonly Histogram PolicyEvaluationDuration = Metrics.CreateHistogram(
        "policy_evaluation_duration_seconds",
        "Duration of policy evaluations in seconds",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.0001, 2, 14)
        });

    private static readonly Histogram EngineExecutionDuration = Metrics.CreateHistogram(
        "engine_execution_duration_seconds",
        "Duration of engine executions in seconds",
        new HistogramConfiguration
        {
            LabelNames = new[] { "engine_tier", "engine_name" },
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 16)
        });

    public MetricsMiddleware(RequestDelegate next)
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

    public static IDisposable TrackPolicyEvaluation()
    {
        return PolicyEvaluationDuration.NewTimer();
    }

    public static IDisposable TrackEngineExecution(string tier, string engineName)
    {
        return EngineExecutionDuration.WithLabels(tier, engineName).NewTimer();
    }
}
