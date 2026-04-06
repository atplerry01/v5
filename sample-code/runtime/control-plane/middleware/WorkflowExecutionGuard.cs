using Whycespace.Runtime.Command;
using Whycespace.Runtime.ControlPlane.Policy;

namespace Whycespace.Runtime.ControlPlane.Middleware;

/// <summary>
/// Middleware guard for workflow operations.
/// Ensures a PolicyDecision is present before allowing workflow step execution.
/// Returns WORKFLOW.NO_POLICY if missing, WORKFLOW.TRANSITION_DENIED if denied.
/// </summary>
public sealed class WorkflowExecutionGuard
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
                "Workflow step requires policy decision",
                "WORKFLOW.NO_POLICY"));
        }

        if (decision.Result != Policy.PolicyDecisionResult.Allow)
        {
            return Task.FromResult(CommandResult.Fail(
                context.Envelope.CommandId,
                decision.DenialReason ?? "Workflow transition denied by policy",
                "WORKFLOW.TRANSITION_DENIED"));
        }

        return next(context);
    }
}
