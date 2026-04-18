using Whycespace.Shared.Contracts.Infrastructure.Policy;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Phase 8 B7 — records every <see cref="IPolicyEvaluator.EvaluateAsync"/>
/// call so tests can inspect the enriched <see cref="PolicyContext"/> the
/// B6 middleware constructed. Returns a configurable
/// <see cref="PolicyDecision"/> so callers can exercise both allow and
/// deny paths (including deny with a canned <c>DenialReason</c> to
/// validate <c>CommandResult.PolicyDenyReason</c> propagation).
///
/// <para>
/// Distinct from <c>AllowAllPolicyEvaluator</c> (which only toggles
/// allow/deny) — this double preserves the exact argument list passed to
/// it so the test can assert over
/// <c>Calls[0].PolicyContext.Command</c>,
/// <c>Calls[0].PolicyContext.ResourceState</c>,
/// <c>Calls[0].PolicyContext.Now</c>, etc.
/// </para>
/// </summary>
public sealed class CapturingPolicyEvaluator : IPolicyEvaluator
{
    public sealed record Call(string PolicyId, object Command, PolicyContext PolicyContext);

    private readonly List<Call> _calls = new();

    public IReadOnlyList<Call> Calls => _calls;

    /// <summary>
    /// Decision returned to every caller. Default is allow; tests can
    /// override to drive the deny path. The supplied
    /// <see cref="PolicyDecision.DenialReason"/> is used as-is so deny
    /// tests can assert on the propagated reason.
    /// </summary>
    public PolicyDecision? NextDecision { get; set; }

    public Task<PolicyDecision> EvaluateAsync(
        string policyId,
        object command,
        PolicyContext policyContext)
    {
        _calls.Add(new Call(policyId, command, policyContext));

        var decision = NextDecision
            ?? new PolicyDecision(
                IsAllowed: true,
                PolicyId: policyId,
                DecisionHash: "capture-allow",
                DenialReason: null);

        return Task.FromResult(decision);
    }
}
