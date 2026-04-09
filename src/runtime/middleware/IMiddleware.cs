using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Middleware;

/// <summary>
/// Runtime middleware interface. All middleware in the control plane pipeline
/// implements this contract. Execution order is locked by RuntimeControlPlaneBuilder.
///
/// phase1.5-S5.2.3 / TC-1 (DISPATCHER-CT-CONTRACT-01): the contract
/// now carries a CancellationToken end-to-end. The <c>next</c>
/// delegate shape is <c>Func&lt;CancellationToken, Task&lt;CommandResult&gt;&gt;</c>
/// so each middleware forwards the token via <c>next(cancellationToken)</c>.
/// Pre-TC-1 the contract was token-free, which made the entire
/// dispatcher pipeline uncancelable from the API edge. The locked
/// middleware order is unchanged.
/// </summary>
public interface IMiddleware
{
    Task<CommandResult> ExecuteAsync(
        CommandContext context,
        object command,
        Func<CancellationToken, Task<CommandResult>> next,
        CancellationToken cancellationToken = default);
}
