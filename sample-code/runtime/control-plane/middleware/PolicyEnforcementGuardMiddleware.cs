using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Policy;

namespace Whycespace.Runtime.ControlPlane.Middleware;

/// <summary>
/// Policy Enforcement Lock (PEL) middleware.
/// Ensures every command has an explicit PolicyDecision before execution.
/// - Missing decision: POLICY_DECISION_MISSING
/// - Deny decision: POLICY_ENFORCEMENT_DENIED
/// - Conditional with unmet conditions: POLICY_CONDITION_UNMET
/// - Allow or conditional with met conditions: proceed to next
/// </summary>
public sealed class PolicyEnforcementGuardMiddleware : IMiddleware
{
    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        var decision = context.Get<PolicyDecision>(PolicyDecision.ContextKey);

        if (decision is null)
        {
            return CommandResult.Fail(
                context.Envelope.CommandId,
                "POLICY ENFORCEMENT LOCK: No policy decision attached to command context",
                "POLICY_DECISION_MISSING");
        }

        switch (decision.Result)
        {
            case PolicyDecisionResult.Deny:
                context.Set("Policy.Violation", new
                {
                    DecisionId = decision.DecisionId,
                    Reason = decision.DenialReason,
                    CommandType = context.Envelope.CommandType,
                    Timestamp = decision.Timestamp
                });
                return CommandResult.Fail(
                    context.Envelope.CommandId,
                    $"POLICY DENIED: {decision.DenialReason ?? "Access denied by policy"}",
                    "POLICY_ENFORCEMENT_DENIED");

            case PolicyDecisionResult.Conditional:
                if (decision.Conditions is { Count: > 0 })
                {
                    foreach (var condition in decision.Conditions)
                    {
                        if (!context.Properties.ContainsKey(condition))
                        {
                            return CommandResult.Fail(
                                context.Envelope.CommandId,
                                $"Condition not met: {condition}",
                                "POLICY_CONDITION_UNMET");
                        }
                    }
                }
                break;

            case PolicyDecisionResult.Allow:
                break;
        }

        context.Set("Policy.EnforcedDecisionId", decision.DecisionId.ToString());
        return await next(context);
    }
}
