using Whycespace.Runtime.Middleware;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Runtime.Middleware.PrePolicy;

/// <summary>
/// Pre-policy validation: rejects malformed commands early before any policy or engine cost.
/// Validates command envelope structural integrity per WBSM v3.5 standard.
/// </summary>
public sealed class ValidationMiddleware : IMiddleware
{
    public Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default)
    {
        if (command is null)
            return Task.FromResult(CommandResult.ValidationFailure(
                "Command payload is required.", ValidationFailureCategory.InputSchema));

        if (context.CommandId == Guid.Empty)
            return Task.FromResult(CommandResult.ValidationFailure(
                "CommandId is required.", ValidationFailureCategory.CommandPrecondition));

        if (context.CausationId == Guid.Empty)
            return Task.FromResult(CommandResult.ValidationFailure(
                "CausationId is required.", ValidationFailureCategory.CommandPrecondition));

        var commandType = command.GetType();
        if (commandType.Namespace is null || !commandType.IsClass)
            return Task.FromResult(CommandResult.ValidationFailure(
                $"Invalid command type: {commandType.Name}", ValidationFailureCategory.InputSchema));

        return next(cancellationToken);
    }
}
