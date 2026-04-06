using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Systems.Upstream.Governance.Audit;

public sealed class GovernanceAuditService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly IClock _clock;

    public GovernanceAuditService(ISystemIntentDispatcher intentDispatcher, IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _clock = clock;
    }

    public async Task LogDecisionAsync(
        string proposalId,
        string outcome,
        string actionType,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        // Log to WhyceChain — immutable audit trail
        await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"GovernanceAuditService:LogDecision:{proposalId}:{outcome}:{actionType}"),
            CommandType = "whycechain.log.governance-decision",
            Payload = new
            {
                ProposalId = proposalId,
                Outcome = outcome,
                ActionType = actionType,
                Timestamp = _clock.UtcNowOffset
            },
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);

        // Store decision hash for tamper detection
        await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"GovernanceAuditService:HashStore:{proposalId}:{outcome}"),
            CommandType = "whycechain.hash.store",
            Payload = new
            {
                RecordType = "governance-decision",
                RecordId = proposalId,
                Outcome = outcome
            },
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }

    public async Task LogPolicyEnforcementAsync(
        string commandType,
        string policyId,
        bool allowed,
        string? reason,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"GovernanceAuditService:LogPolicyEnforcement:{commandType}:{policyId}:{allowed}"),
            CommandType = "whycechain.log.policy-enforcement",
            Payload = new
            {
                CommandType = commandType,
                PolicyId = policyId,
                Allowed = allowed,
                Reason = reason,
                Timestamp = _clock.UtcNowOffset
            },
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);
    }
}
