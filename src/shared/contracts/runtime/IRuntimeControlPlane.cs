namespace Whyce.Shared.Contracts.Runtime;

public interface IRuntimeControlPlane
{
    // phase1.5-S5.2.3 / TC-1 (DISPATCHER-CT-CONTRACT-01): the
    // control plane now accepts a CancellationToken so the dispatcher
    // pipeline can be cancelled by the API edge or host shutdown.
    // Token propagates through the locked middleware order via the
    // updated IMiddleware.ExecuteAsync signature.
    Task<CommandResult> ExecuteAsync(object command, CommandContext context, CancellationToken cancellationToken = default);
}
