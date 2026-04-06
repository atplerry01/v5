using Whycespace.Shared.Primitives.Id;
using Whycespace.Platform.Adapters;
using Whycespace.Platform.Middleware;

namespace Whycespace.Platform.Api.Decision.Governance.Dispute;

public sealed class DisputeController
{
    private const string CommandPrefix = "decision.governance.dispute";

    private readonly DownstreamAdapter _downstream;
    private readonly ProjectionAdapter _projections;

    public DisputeController(DownstreamAdapter downstream, ProjectionAdapter projections)
    {
        _downstream = downstream;
        _projections = projections;
    }

    public Task<ApiResponse> CreateAsync(DisputeRequest request, ApiRequest context)
    {
        var entityId = request.EntityId ?? DeterministicIdHelper.FromSeed($"{context.RequestId}:{CommandPrefix}").ToString();

        return _downstream.SendCommandAsync(
            $"{CommandPrefix}.create",
            new { EntityId = entityId, request.Payload },
            context.RequestId,
            context.WhyceId,
            traceId: context.TraceId,
            aggregateId: entityId);
    }

    public async Task<ApiResponse> GetByIdAsync(string entityId, ApiRequest context)
    {
        var result = await _projections.QueryAsync<DisputeResponse>(
            CommandPrefix,
            new Dictionary<string, object> { ["aggregateId"] = entityId },
            context.TraceId);

        if (result.Data is null)
            return ApiResponse.NotFound($"Dispute {entityId} not found.", context.TraceId);

        return result;
    }
}
