using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Middleware;

namespace Whycespace.Engines.T0U.WhyceId.Federation;

/// <summary>
/// FIX 2 — Enforces step-up requirements for CONDITIONAL federation decisions.
///
/// When a policy decision is CONDITIONAL:
///   - Checks if required conditions are met (step-up auth, additional verification)
///   - Blocks execution until conditions are satisfied
///   - Attaches required actions to context for upstream response
///
/// NO persistence. NO business logic. Pure enforcement only.
/// </summary>
public sealed class ConditionalExecutionHandler : IMiddleware
{
    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var isConditional = context.Properties.TryGetValue(
            FederationPolicyMiddleware.ContextKeys.ConditionalFlow, out var val) && val is true;

        if (!isConditional)
            return await next(context);

        // Check if step-up conditions are already satisfied
        var stepUpSatisfied = context.Properties.TryGetValue(
            ContextKeys.StepUpSatisfied, out var satisfied) && satisfied is true;

        if (stepUpSatisfied)
            return await next(context);

        // Determine required actions based on policy decision
        var requiredActions = DetermineRequiredActions(context);
        context.Set(ContextKeys.RequiredActions, requiredActions);

        // Block execution — return CONDITIONAL response with required actions
        return CommandResult.Fail(
            context.Envelope.CommandId,
            $"Federation conditional: step-up required. Actions: {string.Join(", ", requiredActions)}",
            "FEDERATION_CONDITIONAL_STEP_UP_REQUIRED",
            context.Clock.UtcNowOffset);
    }

    private static List<string> DetermineRequiredActions(CommandContext context)
    {
        var actions = new List<string>();

        var policyDecision = context.Get<FederationPolicyDecision>(
            FederationPolicyMiddleware.ContextKeys.PolicyDecision);

        if (policyDecision?.Reason?.Contains("System-inferred", StringComparison.OrdinalIgnoreCase) == true)
            actions.Add("manual_review_required");

        if (policyDecision?.Reason?.Contains("degraded", StringComparison.OrdinalIgnoreCase) == true)
            actions.Add("enhanced_verification_required");

        if (actions.Count == 0)
            actions.Add("step_up_authentication_required");

        return actions;
    }

    public static class ContextKeys
    {
        public const string StepUpSatisfied = "Federation.StepUpSatisfied";
        public const string RequiredActions = "Federation.RequiredActions";
    }
}
