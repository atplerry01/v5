using Whyce.Shared.Contracts.Infrastructure.Policy;

namespace Whycespace.Tests.Integration.Setup;

/// <summary>
/// Test substitute for the OPA-backed IPolicyEvaluator. Returns a fixed
/// allow decision with a deterministic decision hash so the OPA layer can
/// be exercised by the real PolicyMiddleware without an OPA process.
/// The constitutional WhycePolicyEngine still runs after this.
/// </summary>
public sealed class AllowAllPolicyEvaluator : IPolicyEvaluator
{
    public bool ShouldDeny { get; set; }

    public Task<PolicyDecision> EvaluateAsync(string policyId, object command, PolicyContext policyContext)
    {
        if (ShouldDeny)
            return Task.FromResult(new PolicyDecision(false, policyId, "test-deny", "Denied by test"));

        return Task.FromResult(new PolicyDecision(true, policyId, "test-allow", null));
    }
}
