using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Platform.Middleware;

public sealed class AuthenticationMiddleware : IApiMiddleware
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IClock _clock;

    public AuthenticationMiddleware(IRuntimeControlPlane controlPlane, IClock clock)
    {
        _controlPlane = controlPlane;
        _clock = clock;
    }

    public async Task<ApiResponse> InvokeAsync(ApiRequest request, Func<ApiRequest, Task<ApiResponse>> next)
    {
        if (string.IsNullOrWhiteSpace(request.BearerToken))
            return ApiResponse.Unauthorized(request.TraceId);

        var authResult = await _controlPlane.ExecuteAsync(new RuntimeCommandEnvelope
        {
            CommandId = DeterministicIdHelper.FromSeed($"authn:{request.RequestId}:{request.BearerToken}"),
            CommandType = "whyceid.authenticate",
            Payload = new { Token = request.BearerToken },
            CorrelationId = request.RequestId,
            Timestamp = _clock.UtcNowOffset
        });

        if (!authResult.Success)
            return ApiResponse.Unauthorized(request.TraceId);

        var authenticatedRequest = request with { WhyceId = authResult.Data?.ToString() };
        return await next(authenticatedRequest);
    }
}
