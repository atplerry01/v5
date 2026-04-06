using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Midstream.WhyceAtlas.Orchestration;

/// <summary>
/// Delegates workforce optimization to engines via intent dispatch.
/// No local scoring/ranking — all business logic lives in engines.
/// </summary>
public sealed class WorkforceOptimizationService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly IClock _clock;

    public WorkforceOptimizationService(ISystemIntentDispatcher intentDispatcher, IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _clock = clock;
    }

    public async Task<IntentResult> OptimizeAsync(
        string clusterId,
        string taskType,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"WorkforceOptimizationService:OptimizeAsync:{clusterId}:{taskType}"),
            CommandType = "atlas.optimize.workforce",
            Payload = new { ClusterId = clusterId, TaskType = taskType },
            CorrelationId = DeterministicIdHelper.FromSeed($"correlation:WorkforceOptimizationService:OptimizeAsync:{clusterId}:{taskType}").ToString(),
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }

    public async Task<IntentResult> QueryCrossClusterAvailabilityAsync(
        string taskType,
        IReadOnlyList<string> clusterIds,
        CancellationToken cancellationToken = default)
    {
        var clusterKey = string.Join(",", clusterIds);
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"WorkforceOptimizationService:QueryCrossClusterAvailabilityAsync:{taskType}:{clusterKey}"),
            CommandType = "atlas.query.cross-cluster-availability",
            Payload = new { TaskType = taskType, ClusterIds = clusterIds },
            CorrelationId = DeterministicIdHelper.FromSeed($"correlation:WorkforceOptimizationService:QueryCrossClusterAvailabilityAsync:{taskType}:{clusterKey}").ToString(),
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }
}
