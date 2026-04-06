using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Middleware;

public sealed class AuthorizationMiddleware : IApiMiddleware
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IClock _clock;

    public AuthorizationMiddleware(IRuntimeControlPlane controlPlane, IClock clock)
    {
        _controlPlane = controlPlane;
        _clock = clock;
    }

    public async Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        if (string.IsNullOrWhiteSpace(request.WhyceId))
            return ApiResponse.Unauthorized(request.TraceId);

        var authzResult = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = DeterministicIdHelper.FromSeed($"authz:{request.RequestId}:{request.Endpoint}"),
            CommandType = "policy.authorize",
            Payload = new { WhyceId = request.WhyceId, Endpoint = request.Endpoint, Method = request.Method },
            CorrelationId = request.RequestId,
            Timestamp = _clock.UtcNowOffset,
            WhyceId = request.WhyceId
        });

        if (!authzResult.Success)
            return ApiResponse.Forbidden(authzResult.ErrorMessage ?? "Access denied", request.TraceId);

        return await next(request);
    }
}
