using Whycespace.Shared.Primitives.Id;
using Whycespace.Platform.Adapters;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Trust.Identity.Identity;

public sealed class IdentityController
{
    private const string CommandPrefix = "identity";
    private const string ProjectionName = "identity";
    private readonly DownstreamAdapter _downstream;
    private readonly ProjectionAdapter _projections;

    public IdentityController(DownstreamAdapter downstream, ProjectionAdapter projections)
    { _downstream = downstream; _projections = projections; }

    public Task<ApiResponse> RegisterAsync(RegisterIdentityDto dto, ApiRequest context)
    {
        var identityId = dto.IdentityId ?? DeterministicIdHelper.FromSeed($"{context.RequestId}:{CommandPrefix}").ToString();
        return _downstream.SendCommandAsync(
            $"{CommandPrefix}.create", new { IdentityId = identityId, dto.IdentityType, dto.DisplayName },
            context.RequestId, context.WhyceId, traceId: context.TraceId, aggregateId: identityId);
    }

    public Task<ApiResponse> ActivateAsync(ActivateIdentityDto dto, ApiRequest context) =>
        _downstream.SendCommandAsync($"{CommandPrefix}.activate", new { dto.IdentityId },
            context.RequestId, context.WhyceId, traceId: context.TraceId, aggregateId: dto.IdentityId);

    public async Task<ApiResponse> GetByIdAsync(string identityId, ApiRequest context)
    {
        var result = await _projections.QueryAsync<IdentityQueryResponse>(
            ProjectionName, new Dictionary<string, object> { ["aggregateId"] = identityId }, context.TraceId);
        if (result.Data is null) return ApiResponse.NotFound($"Identity {identityId} not found.", context.TraceId);
        return result;
    }

    public Task<ApiResponse> ListAsync(ApiRequest context) =>
        _projections.QueryListAsync<IdentityQueryResponse>(ProjectionName, new Dictionary<string, object>(), context.TraceId);
}
