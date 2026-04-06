using Whycespace.Shared.Contracts.Governance;
using Whycespace.Shared.Contracts.Systems.Intent;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;
using Whycespace.Systems.Upstream.Governance.Audit;

namespace Whycespace.Systems.Upstream.WhycePolicy.Enforcement;

public sealed class PolicyEnforcementService
{
    private readonly ISystemIntentDispatcher _intentDispatcher;
    private readonly GovernanceAuditService _audit;
    private readonly IClock _clock;

    public PolicyEnforcementService(ISystemIntentDispatcher intentDispatcher, GovernanceAuditService audit, IClock clock)
    {
        _intentDispatcher = intentDispatcher;
        _audit = audit;
        _clock = clock;
    }

    public async Task<PolicyDecisionResult> EvaluateAsync(
        string commandType,
        object payload,
        string? policyId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"PolicyEnforcementService:EvaluateAsync:{commandType}:{policyId}:{correlationId}"),
            CommandType = "policy.evaluate",
            Payload = new { CommandType = commandType, Payload = payload, PolicyId = policyId },
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset,
            PolicyId = policyId
        }, cancellationToken);

        var decision = new PolicyDecisionResult
        {
            Allowed = result.Success,
            Reason = result.Success ? "Policy evaluation passed" : result.ErrorMessage,
            PolicyId = policyId,
            CommandType = commandType,
            RequiresGuardianApproval = Tier0ActionClassifier.IsTier0Action(commandType)
        };

        await _audit.LogPolicyEnforcementAsync(
            commandType, policyId ?? "default", decision.Allowed, decision.Reason,
            correlationId, cancellationToken);

        return decision;
    }

    public async Task<IntentResult> EnforceAndExecuteAsync(
        string commandType,
        object payload,
        string? policyId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var decision = await EvaluateAsync(commandType, payload, policyId, correlationId, cancellationToken);

        if (!decision.Allowed)
            return IntentResult.Fail(Guid.Empty, decision.Reason ?? "Policy denied.", "POLICY_DENIED");

        if (decision.RequiresGuardianApproval)
            return IntentResult.Fail(Guid.Empty,
                $"Tier-0 action '{commandType}' requires guardian approval.",
                "GOVERNANCE_GUARDIAN_REQUIRED");

        return await _intentDispatcher.DispatchAsync(new ExecuteCommandIntent
        {
            CommandId = DeterministicIdHelper.FromSeed($"PolicyEnforcementService:EnforceAndExecuteAsync:{commandType}:{policyId}:{correlationId}"),
            CommandType = commandType,
            Payload = payload,
            CorrelationId = correlationId,
            Timestamp = _clock.UtcNowOffset,
            PolicyId = policyId
        }, cancellationToken);
    }

}

public sealed record PolicyDecisionResult
{
    public required bool Allowed { get; init; }
    public string? Reason { get; init; }
    public string? PolicyId { get; init; }
    public required string CommandType { get; init; }
    public required bool RequiresGuardianApproval { get; init; }
}
