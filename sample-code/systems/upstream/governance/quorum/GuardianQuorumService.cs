using Whycespace.Shared.Contracts.Governance;
using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Upstream.Governance.Quorum;

/// <summary>
/// Orchestration-only wrapper. Delegates quorum evaluation via intent dispatch.
/// Systems layer does NOT compute — it routes through the runtime pipeline.
/// </summary>
public sealed class GuardianQuorumService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly IClock _clock;

    public GuardianQuorumService(ISystemIntentDispatcher intentDispatcher, IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _clock = clock;
    }

    public async Task<IntentResult> EvaluateAsync(
        IReadOnlyList<GuardianVote> votes,
        QuorumThreshold threshold,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"GuardianQuorumService:EvaluateAsync:{correlationId}:{threshold.MinimumVoters}:{threshold.RequiredApprovals}"),
            CommandType = "governance.quorum.evaluate",
            Payload = new
            {
                Votes = votes,
                MinimumVoters = threshold.MinimumVoters,
                RequiredApprovals = threshold.RequiredApprovals
            },
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }
}
