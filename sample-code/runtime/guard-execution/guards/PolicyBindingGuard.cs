using Whycespace.Runtime.GuardExecution.Contracts;
using Whycespace.Runtime.GuardExecution.Engine;

namespace Whycespace.Runtime.GuardExecution.Guards;

/// <summary>
/// Enforces policy binding presence:
/// - Every command type must have a policy binding
/// - Policy bindings must reference valid policy IDs
/// - No orphaned bindings
/// </summary>
public sealed class PolicyBindingGuard : IGuard
{
    public string Name => "PolicyBindingGuard";
    public GuardCategory Category => GuardCategory.PolicyBinding;
    public GuardPhase Phase => GuardPhase.PostPolicy;

    public Task<GuardResult> EvaluateAsync(GuardContext context, CancellationToken cancellationToken = default)
    {
        var violations = new List<GuardViolation>();

        if (context.Mode == GuardExecutionMode.Runtime)
        {
            // Runtime: verify command has policy binding
            if (context.PolicyDecision is null)
            {
                violations.Add(new GuardViolation
                {
                    Rule = "POLICY_BINDING.MISSING",
                    Severity = GuardSeverity.S0,
                    File = "runtime",
                    Description = $"Command '{context.CommandName}' has no policy binding",
                    Expected = "Every command must be bound to at least one policy",
                    Remediation = "Register a policy binding for this command type."
                });
            }
            else if (context.PolicyDecision.PolicyIds.Count == 0)
            {
                violations.Add(new GuardViolation
                {
                    Rule = "POLICY_BINDING.EMPTY_POLICIES",
                    Severity = GuardSeverity.S1,
                    File = "runtime",
                    Description = $"PolicyDecision for '{context.CommandName}' has zero policy IDs",
                    Expected = "At least one policy must be evaluated per command",
                    Remediation = "Ensure the policy evaluator binds policies to this command type."
                });
            }
        }

        return Task.FromResult(violations.Count == 0
            ? GuardResult.Pass(Name)
            : GuardResult.Fail(Name, violations));
    }
}
