using Whycespace.Shared.Contracts.Governance;
using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;
using Whycespace.Systems.Upstream.Governance.Audit;

namespace Whycespace.Systems.Upstream.Governance.Approval;

/// <summary>
/// Guardian approval orchestration.
/// Systems layer — routes and composes, does NOT decide.
/// Decision logic delegated via intent dispatch to runtime → T1M → T3I.
/// Audit logging delegated to GovernanceAuditService.
/// Execution delegated via intent dispatch to runtime boundary.
/// </summary>
public sealed class ApprovalWorkflow
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly GovernanceAuditService _audit;
    private readonly IClock _clock;

    public ApprovalWorkflow(
        ISystemIntentDispatcher intentDispatcher,
        GovernanceAuditService audit,
        IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _audit = audit;
        _clock = clock;
    }

    public async Task<IntentResult> ExecuteAsync(
        string proposalId,
        string actionType,
        object actionPayload,
        IReadOnlyList<GuardianVote> votes,
        QuorumThreshold threshold,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Delegate decision evaluation via intent dispatch to runtime → T1M engine
        var decisionResult = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"ApprovalWorkflow:Evaluate:{proposalId}:{actionType}:{correlationId}"),
            CommandType = "governance.approval.evaluate",
            Payload = new
            {
                ProposalId = proposalId,
                ActionType = actionType,
                Votes = votes,
                MinimumVoters = threshold.MinimumVoters,
                RequiredApprovals = threshold.RequiredApprovals
            },
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);

        // Step 2: Route based on decision outcome
        if (!decisionResult.Success)
        {
            await _audit.LogDecisionAsync(proposalId, decisionResult.ErrorCode ?? "DENIED",
                actionType, correlationId, cancellationToken);

            return decisionResult;
        }

        // Step 3: Approved — dispatch execution intent to runtime boundary
        var executionResult = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"ApprovalWorkflow:ExecuteAsync:{proposalId}:{actionType}:{correlationId}"),
            CommandType = actionType,
            Payload = actionPayload,
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset
        }, cancellationToken);

        // Step 4: Log audit trail
        await _audit.LogDecisionAsync(proposalId, "APPROVED_AND_EXECUTED", actionType, correlationId, cancellationToken);

        return executionResult;
    }
}
