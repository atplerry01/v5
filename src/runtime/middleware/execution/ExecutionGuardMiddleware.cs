using Whyce.Runtime.Middleware;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Middleware.Execution;

/// <summary>
/// Final gate before command dispatch to engine.
/// Marks the command context as runtime-originated (RuntimeOriginKey).
/// Ensures all preceding guards and policy evaluation have passed.
/// No command reaches an engine without this guard approving it.
/// </summary>
public sealed class ExecutionGuardMiddleware : IMiddleware
{
    public const string RuntimeOriginKey = "Runtime.IsFromRuntime";

    public Task<CommandResult> ExecuteAsync(CommandContext context, object command, Func<Task<CommandResult>> next)
    {
        // Final validation: policy decision must have been evaluated
        if (context.PolicyDecisionAllowed is not true)
        {
            return Task.FromResult(CommandResult.Failure(
                "Execution guard: command cannot proceed without approved policy decision."));
        }

        if (string.IsNullOrWhiteSpace(context.PolicyDecisionHash))
        {
            return Task.FromResult(CommandResult.Failure(
                "Execution guard: policy decision hash is required for chain anchoring."));
        }

        // Mark context as runtime-originated — engines can verify this
        context.RuntimeOrigin = true;

        return next();
    }
}
