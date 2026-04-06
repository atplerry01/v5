using Whyce.Runtime.Middleware;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Middleware.PostPolicy;

/// <summary>
/// Post-policy authorization guard. Runs AFTER policy evaluation.
/// Validates that the resolved identity has the required authorization context
/// for the specific command being executed.
/// </summary>
public sealed class AuthorizationGuardMiddleware : IMiddleware
{
    public Task<CommandResult> ExecuteAsync(CommandContext context, object command, Func<Task<CommandResult>> next)
    {
        // Authorization requires identity to have been resolved by PolicyMiddleware
        if (string.IsNullOrWhiteSpace(context.IdentityId))
            return Task.FromResult(CommandResult.Failure("Authorization failed: no identity resolved."));

        // Policy decision must exist and be allowed
        if (context.PolicyDecisionAllowed is not true)
            return Task.FromResult(CommandResult.Failure("Authorization failed: policy decision not approved."));

        if (string.IsNullOrWhiteSpace(context.PolicyDecisionHash))
            return Task.FromResult(CommandResult.Failure("Authorization failed: policy decision hash missing."));

        return next();
    }
}
