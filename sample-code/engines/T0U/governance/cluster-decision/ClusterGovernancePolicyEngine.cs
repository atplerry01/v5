using Whycespace.Shared.Contracts.Policy;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Engines.T0U.Governance.ClusterDecision;

/// <summary>
/// E18.7.2 — Policy-driven decision engine for cluster governance.
/// Evaluates governance decisions against WHYCEPOLICY.
/// Policy is the ONLY authority — no override by intelligence.
/// </summary>
public sealed class ClusterGovernancePolicyEngine : GovernanceEngineBase
{
    private readonly IPolicyEvaluator _policyEvaluator;
    private readonly IClock _clock;

    public ClusterGovernancePolicyEngine(IPolicyEvaluator policyEvaluator, IClock clock)
    {
        _policyEvaluator = policyEvaluator;
        _clock = clock;
    }

    public async Task<PolicyEvaluationResult> EvaluateAsync(
        ClusterGovernanceInput input,
        CancellationToken ct = default)
    {
        Guid.TryParse(input.IdentityId, out var actorGuid);

        var policyInput = new PolicyEvaluationInput(
            PolicyId: null,
            ActorId: actorGuid,
            Action: $"governance.{input.DecisionType}",
            Resource: $"cluster:{input.ClusterId}",
            Environment: "governance",
            Timestamp: _clock.UtcNowOffset)
        {
            DomainContext = input
        };

        return await _policyEvaluator.EvaluateAsync(policyInput, ct);
    }
}
