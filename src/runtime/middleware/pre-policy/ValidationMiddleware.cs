using Whyce.Runtime.Middleware;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Middleware.PrePolicy;

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
            return Task.FromResult(CommandResult.Failure("Command payload is required."));

        if (context.CommandId == Guid.Empty)
            return Task.FromResult(CommandResult.Failure("CommandId is required."));

        if (context.CausationId == Guid.Empty)
            return Task.FromResult(CommandResult.Failure("CausationId is required."));

        var commandType = command.GetType();
        if (commandType.Namespace is null || !commandType.IsClass)
            return Task.FromResult(CommandResult.Failure($"Invalid command type: {commandType.Name}"));

        return next(cancellationToken);
    }
}
