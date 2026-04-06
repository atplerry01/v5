using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Midstream.WhyceAtlas.Orchestration;

/// <summary>
/// Delegates risk assessment to the T3I monitoring engine via intent dispatch.
/// No local risk scoring — all business logic lives in engines.
/// </summary>
public sealed class RiskAnalysisService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly IClock _clock;

    public RiskAnalysisService(ISystemIntentDispatcher intentDispatcher, IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _clock = clock;
    }

    public async Task<IntentResult> AssessAsync(
        string clusterId,
        string operationType,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"RiskAnalysisService:AssessAsync:{clusterId}:{operationType}"),
            CommandType = "atlas.assess.risk",
            Payload = new { ClusterId = clusterId, OperationType = operationType },
            CorrelationId = DeterministicIdHelper.FromSeed($"correlation:RiskAnalysisService:{clusterId}:{operationType}").ToString(),
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }
}
