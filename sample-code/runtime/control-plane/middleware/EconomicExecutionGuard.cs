using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Policy;

namespace Whycespace.Runtime.ControlPlane.Middleware;

/// <summary>
/// Middleware guard for economic operations.
/// Ensures a PolicyDecision is present on the command context before allowing execution.
/// Returns ECONOMIC.NO_POLICY if missing, ECONOMIC.POLICY_DENIED if denied.
/// </summary>
public sealed class EconomicExecutionGuard
{
    public Task<CommandResult> InvokeAsync(
        CommandContext context,
        Func<CommandContext, Task<CommandResult>> next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var decision = context.Get<PolicyDecision>(PolicyDecision.ContextKey);
        if (decision is null)
        {
            return Task.FromResult(CommandResult.Fail(
                context.Envelope.CommandId,
                "Economic command requires policy decision",
                "ECONOMIC.NO_POLICY"));
        }

        if (decision.Result != Policy.PolicyDecisionResult.Allow)
        {
            return Task.FromResult(CommandResult.Fail(
                context.Envelope.CommandId,
                decision.DenialReason ?? "Policy denied economic operation",
                "ECONOMIC.POLICY_DENIED"));
        }

        return next(context);
    }
}
