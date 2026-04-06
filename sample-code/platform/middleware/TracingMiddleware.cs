using Whycespace.Shared.Contracts.Observability;

namespace Whycespace.Platform.Middleware;

public sealed class TracingMiddleware : IApiMiddleware
{
    private readonly ITraceService _traceService;

    public TracingMiddleware(ITraceService traceService)
    {
        _traceService = traceService;
    }

    public async Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        var trace = _traceService.StartTrace(request.RequestId, $"api.{request.Endpoint}");
        var tracedRequest = request with { TraceId = trace.TraceId };

        var response = await next(tracedRequest);

        _traceService.CompleteTrace(trace, response.StatusCode < 400);

        return response with { TraceId = trace.TraceId };
    }
}
