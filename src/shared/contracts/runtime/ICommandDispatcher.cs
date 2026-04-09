namespace Whyce.Shared.Contracts.Runtime;

public interface ICommandDispatcher
{
    // phase1.5-S5.2.3 / TC-1 (DISPATCHER-CT-CONTRACT-01): the
    // dispatcher contract now carries a CancellationToken end-to-end
    // so the API edge (HttpContext.RequestAborted) and host shutdown
    // (IHostApplicationLifetime.ApplicationStopping) can propagate
    // through every middleware to the engine layer. Default value
    // preserves source compatibility for any caller that has not
    // yet migrated; the runtime composition root passes a real
    // token from the controller through SystemIntentDispatcher and
    // RuntimeControlPlane.
    Task<CommandResult> DispatchAsync(object command, CommandContext context, CancellationToken cancellationToken = default);
}
