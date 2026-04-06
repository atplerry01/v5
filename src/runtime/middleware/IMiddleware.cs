using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Middleware;

/// <summary>
/// Runtime middleware interface. All middleware in the control plane pipeline
/// implements this contract. Execution order is locked by RuntimeControlPlaneBuilder.
/// </summary>
public interface IMiddleware
{
    Task<CommandResult> ExecuteAsync(CommandContext context, object command, Func<Task<CommandResult>> next);
}
