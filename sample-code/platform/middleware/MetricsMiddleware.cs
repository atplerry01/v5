using System.Diagnostics;
using Whycespace.Shared.Contracts.Observability;

namespace Whycespace.Platform.Middleware;

public sealed class MetricsMiddleware : IApiMiddleware
{
    private readonly IMetricsCollector _metrics;

    public MetricsMiddleware(IMetricsCollector metrics)
    {
        _metrics = metrics;
    }

    public async Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        var sw = Stopwatch.StartNew();

        var response = await next(request);

        sw.Stop();

        _metrics.RecordExecutionTime($"api.{request.Endpoint}", sw.Elapsed);
        _metrics.RecordThroughput($"api.{request.Method}.{request.Endpoint}");
        _metrics.RecordWorkflowOutcome($"api.{request.Endpoint}", response.StatusCode < 400);

        return response;
    }
}
