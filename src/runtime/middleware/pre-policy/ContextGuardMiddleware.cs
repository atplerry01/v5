using Whyce.Runtime.Middleware;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Middleware.PrePolicy;

/// <summary>
/// Pre-policy guard: validates that required execution context fields are present.
/// Runs BEFORE policy evaluation. Missing context = hard stop.
/// </summary>
public sealed class ContextGuardMiddleware : IMiddleware
{
    public Task<CommandResult> ExecuteAsync(CommandContext context, object command, Func<Task<CommandResult>> next)
    {
        if (context.CorrelationId == Guid.Empty)
            return Task.FromResult(CommandResult.Failure("CorrelationId is required."));

        if (string.IsNullOrWhiteSpace(context.TenantId))
            return Task.FromResult(CommandResult.Failure("TenantId is required."));

        if (string.IsNullOrWhiteSpace(context.ActorId))
            return Task.FromResult(CommandResult.Failure("ActorId is required."));

        if (string.IsNullOrWhiteSpace(context.PolicyId))
            return Task.FromResult(CommandResult.Failure("PolicyId is required."));

        if (context.AggregateId == Guid.Empty)
            return Task.FromResult(CommandResult.Failure("AggregateId is required."));

        return next();
    }
}
