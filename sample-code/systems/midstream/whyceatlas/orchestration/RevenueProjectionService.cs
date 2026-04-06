using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Midstream.WhyceAtlas.Orchestration;

/// <summary>
/// Delegates revenue projection to the T3I forecasting engine via intent dispatch.
/// No local calculations — all business logic lives in engines.
/// </summary>
public sealed class RevenueProjectionService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly IClock _clock;

    public RevenueProjectionService(ISystemIntentDispatcher intentDispatcher, IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _clock = clock;
    }

    public async Task<IntentResult> ProjectAsync(
        string clusterId,
        string spvId,
        int horizonDays,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"RevenueProjectionService:ProjectAsync:{clusterId}:{spvId}:{horizonDays}"),
            CommandType = "atlas.project.revenue",
            Payload = new { ClusterId = clusterId, SpvId = spvId, HorizonDays = horizonDays },
            CorrelationId = DeterministicIdHelper.FromSeed($"correlation:RevenueProjectionService:ProjectAsync:{clusterId}:{spvId}:{horizonDays}").ToString(),
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }

    public async Task<IntentResult> ProjectMultiClusterAsync(
        IReadOnlyList<string> clusterIds,
        int horizonDays,
        CancellationToken cancellationToken = default)
    {
        var clusterKey = string.Join(",", clusterIds);
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"RevenueProjectionService:ProjectMultiClusterAsync:{clusterKey}:{horizonDays}"),
            CommandType = "atlas.project.revenue.multi-cluster",
            Payload = new { ClusterIds = clusterIds, HorizonDays = horizonDays },
            CorrelationId = DeterministicIdHelper.FromSeed($"correlation:RevenueProjectionService:ProjectMultiClusterAsync:{clusterKey}:{horizonDays}").ToString(),
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }
}
