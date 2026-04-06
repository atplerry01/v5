using Whycespace.Shared.Contracts.Governance;
using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Upstream.Governance.Voting;

public sealed class VotingService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly IClock _clock;

    public VotingService(ISystemIntentDispatcher intentDispatcher, IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _clock = clock;
    }

    public async Task<IntentResult> CastVoteAsync(
        string proposalId,
        string guardianId,
        VoteDecision decision,
        string? reason,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"VotingService:CastVoteAsync:{proposalId}:{guardianId}:{decision}"),
            CommandType = "governance.vote.cast",
            Payload = new
            {
                ProposalId = proposalId,
                GuardianId = guardianId,
                Decision = decision.ToString(),
                Reason = reason
            },
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }

    public async Task<IntentResult> InitiateVotingRoundAsync(
        string proposalId,
        string actionType,
        IReadOnlyList<string> guardianIds,
        QuorumThreshold threshold,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"VotingService:InitiateVotingRoundAsync:{proposalId}:{actionType}"),
            CommandType = "governance.voting.initiate",
            Payload = new
            {
                ProposalId = proposalId,
                ActionType = actionType,
                GuardianIds = guardianIds,
                MinimumVoters = threshold.MinimumVoters,
                RequiredApprovals = threshold.RequiredApprovals
            },
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }
}
