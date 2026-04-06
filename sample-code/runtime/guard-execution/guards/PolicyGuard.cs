using Whycespace.Runtime.ControlPlane.Policy;
using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces WHYCEPOLICY integration:
/// - PolicyDecision must exist before execution
/// - Decision must be ALLOW
/// - Missing policy = BLOCK
/// </summary>
public sealed class PolicyGuard : IGuard
{
    public string Name => "PolicyGuard";
    public GuardCategory Category => GuardCategory.Policy;
    public GuardPhase Phase => GuardPhase.PostPolicy;

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        // Policy enforcement is runtime-only
        if (context.Mode != GuardExecutionMode.Runtime)
            return Task.FromResult(GuardResult.Pass(Name));

        var violations = new List<GuardViolation>();

        if (context.PolicyDecision is null)
        {
            violations.Add(new GuardViolation
            {
                Rule = "POLICY.DECISION_MISSING",
                Severity = GuardSeverity.S0,
                File = "runtime",
                Description = "No PolicyDecision found in execution context",
                Expected = "Every command execution must have an associated PolicyDecision",
                Remediation = "Ensure PolicyMiddleware runs before GuardMiddleware in the pipeline."
            });
        }
        else if (context.PolicyDecision.Result != PolicyDecisionResult.Allow)
        {
            violations.Add(new GuardViolation
            {
                Rule = "POLICY.DECISION_DENIED",
                Severity = GuardSeverity.S0,
                File = "runtime",
                Description = $"PolicyDecision is {context.PolicyDecision.Result}: {context.PolicyDecision.DenialReason ?? "no reason"}",
                Expected = "PolicyDecision.Result must be Allow",
                Actual = $"Result is {context.PolicyDecision.Result}",
                Remediation = "Resolve the policy denial before execution."
            });
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
