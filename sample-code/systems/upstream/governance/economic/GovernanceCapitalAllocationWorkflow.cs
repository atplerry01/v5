using Whycespace.Shared.Contracts.Systems;
using Whycespace.Shared.Contracts.Systems.Intent;

namespace Whycespace.Systems.Upstream.Governance.Economic;

/// <summary>
/// Governance-gated capital allocation workflow.
/// Flow: CapitalAllocation → Policy Evaluation → Guardian Approval (if threshold exceeded) → Execution
/// Systems layer — orchestration only, no domain mutation.
/// </summary>
public sealed class GovernanceCapitalAllocationWorkflow
{
    private readonly ISystemIntentDispatcher _intentDispatcher;

    public GovernanceCapitalAllocationWorkflow(ISystemIntentDispatcher intentDispatcher)
    {
        _intentDispatcher = intentDispatcher ?? throw new ArgumentNullException(nameof(intentDispatcher));
    }

    public async Task<IntentResult> ExecuteAsync(
        string capitalAccountId,
        string allocationTarget,
        decimal amount,
        string currencyCode,
        string actorId,
        string clusterId,
        CancellationToken cancellationToken = default)
    {
        var correlationId = Guid.NewGuid().ToString();

        // Step 1: Evaluate policy — does this allocation require governance approval?
        var policyCommandId = Guid.NewGuid();
        var policyResult = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = policyCommandId,
            CommandType = "governance.policy.evaluate",
            Payload = new
            {
                Resource = "economic.capital.allocation",
                Action = "allocate",
                ActorId = actorId,
                Amount = amount,
                CurrencyCode = currencyCode,
                ClusterId = clusterId
            },
            CorrelationId = correlationId,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        if (!policyResult.Success)
            return IntentResult.Fail(policyCommandId, $"Policy evaluation failed: {policyResult.ErrorMessage}");

        // Step 2: If amount exceeds threshold, require governance approval
        var requiresApproval = amount >= 10_000m;

        if (requiresApproval)
        {
            var approvalCommandId = Guid.NewGuid();
            var approvalResult = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
            {
                CommandId = approvalCommandId,
                CommandType = "governance.approval.evaluate",
                Payload = new
                {
                    ProposalType = "CapitalAllocation",
                    CapitalAccountId = capitalAccountId,
                    AllocationTarget = allocationTarget,
                    Amount = amount,
                    CurrencyCode = currencyCode,
                    RequestedBy = actorId,
                    ClusterId = clusterId
                },
                CorrelationId = correlationId,
                Timestamp = DateTimeOffset.UtcNow
            }, cancellationToken);

            if (!approvalResult.Success)
                return IntentResult.Fail(approvalCommandId, $"Governance approval denied: {approvalResult.ErrorMessage}");
        }

        // Step 3: Execute the allocation
        var executionResult = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = Guid.NewGuid(),
            CommandType = "economic.capital.allocate",
            Payload = new
            {
                CapitalAccountId = capitalAccountId,
                AllocationTarget = allocationTarget,
                Amount = amount,
                CurrencyCode = currencyCode
            },
            CorrelationId = correlationId,
            Timestamp = DateTimeOffset.UtcNow
        }, cancellationToken);

        // Step 4: Anchor decision to chain
        if (executionResult.Success)
        {
            await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
            {
                CommandId = Guid.NewGuid(),
                CommandType = "chain.anchor.decision",
                Payload = new
                {
                    DecisionType = "CapitalAllocation",
                    CapitalAccountId = capitalAccountId,
                    Amount = amount,
                    ApprovalRequired = requiresApproval,
                    ActorId = actorId
                },
                CorrelationId = correlationId,
                Timestamp = DateTimeOffset.UtcNow
            }, cancellationToken);
        }

        return executionResult;
    }
}
