using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Middleware.PrePolicy;

/// <summary>
/// Pre-policy guard: validates that required execution context fields are present.
/// Runs BEFORE policy evaluation. Missing context = hard stop.
/// </summary>
public sealed class ContextGuardMiddleware : IMiddleware
{
    public Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        if (context.CorrelationId == Guid.Empty)
            return Task.FromResult(CommandResult.ValidationFailure(
                "CorrelationId is required.", ValidationFailureCategory.CommandPrecondition));

        if (string.IsNullOrWhiteSpace(context.TenantId))
            return Task.FromResult(CommandResult.ValidationFailure(
                "TenantId is required.", ValidationFailureCategory.CommandPrecondition));

        if (string.IsNullOrWhiteSpace(context.ActorId))
            return Task.FromResult(CommandResult.ValidationFailure(
                "ActorId is required.", ValidationFailureCategory.CommandPrecondition));

        if (string.IsNullOrWhiteSpace(context.PolicyId))
            return Task.FromResult(CommandResult.ValidationFailure(
                "PolicyId is required.", ValidationFailureCategory.CommandPrecondition));

        if (context.AggregateId == Guid.Empty)
            return Task.FromResult(CommandResult.ValidationFailure(
                "AggregateId is required.", ValidationFailureCategory.CommandPrecondition));

        return next(cancellationToken);
    }
}
