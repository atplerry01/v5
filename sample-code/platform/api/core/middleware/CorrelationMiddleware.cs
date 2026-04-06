using Whycespace.Platform.Middleware;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Api.Core.Middleware;

/// <summary>
/// Generates a deterministic CorrelationId for every inbound request.
/// Blocks requests that arrive without a RequestId (upstream must assign one).
/// Enriches the request with a TraceId derived from the CorrelationId.
/// </summary>
public sealed class CorrelationMiddleware : IApiMiddleware
{
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;

    public CorrelationMiddleware(IIdGenerator idGenerator, IClock clock)
    {
        _idGenerator = idGenerator;
        _clock = clock;
    }

    public Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        if (string.IsNullOrWhiteSpace(request.RequestId))
            return Task.FromResult(ApiResponse.BadRequest("Missing RequestId — platform requires a RequestId on every request"));

        var correlationId = _idGenerator
            .DeterministicGuid($"correlation:{request.RequestId}:{_clock.UtcNowOffset:O}")
            .ToString("N");

        var traceId = _idGenerator
            .DeterministicGuid($"trace:{correlationId}")
            .ToString("N");

        var enriched = request with
        {
            TraceId = traceId,
            Headers = MergeHeader(request.Headers, "X-Correlation-Id", correlationId)
        };

        return next(enriched);
    }

    private static IReadOnlyDictionary<string, string> MergeHeader(
        IReadOnlyDictionary<string, string> existing, string key, string value)
    {
        var merged = new Dictionary<string, string>(existing) { [key] = value };
        return merged;
    }
}
