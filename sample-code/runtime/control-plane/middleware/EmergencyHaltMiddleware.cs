using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.ControlPlane.Middleware;

/// <summary>
/// Runtime middleware that checks for active emergency halts.
/// Rejects ALL commands when a global halt is active.
/// Rejects region-scoped commands when a region halt is active.
/// This is the first middleware in the pipeline — runs before all others.
/// </summary>
public sealed class EmergencyHaltMiddleware : IMiddleware
{
    private readonly IEmergencyHaltProvider _haltProvider;

    public EmergencyHaltMiddleware(IEmergencyHaltProvider haltProvider)
    {
        ArgumentNullException.ThrowIfNull(haltProvider);
        _haltProvider = haltProvider;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        // Check global halt first
        if (await _haltProvider.IsGlobalHaltActiveAsync(context.CancellationToken))
        {
            return CommandResult.Fail(
                context.Envelope.CommandId,
                "GLOBAL EMERGENCY HALT ACTIVE. All operations suspended.",
                "EMERGENCY_HALT_GLOBAL");
        }

        // Check region halt
        var headers = context.Envelope.Metadata.Headers;
        if (headers.TryGetValue("x-target-region", out var regionId) && !string.IsNullOrWhiteSpace(regionId))
        {
            if (await _haltProvider.IsRegionHaltActiveAsync(regionId, context.CancellationToken))
            {
                return CommandResult.Fail(
                    context.Envelope.CommandId,
                    $"EMERGENCY HALT ACTIVE for region '{regionId}'. Operations suspended.",
                    "EMERGENCY_HALT_REGION");
            }
        }

        // Check SPV halt
        if (headers.TryGetValue("x-spv-id", out var spvId) && !string.IsNullOrWhiteSpace(spvId))
        {
            if (await _haltProvider.IsSpvHaltActiveAsync(spvId, context.CancellationToken))
            {
                return CommandResult.Fail(
                    context.Envelope.CommandId,
                    $"EMERGENCY HALT ACTIVE for SPV '{spvId}'. Operations frozen.",
                    "EMERGENCY_HALT_SPV");
            }
        }

        return await next(context);
    }
}

public interface IEmergencyHaltProvider
{
    Task<bool> IsGlobalHaltActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> IsRegionHaltActiveAsync(string regionId, CancellationToken cancellationToken = default);
    Task<bool> IsSpvHaltActiveAsync(string spvId, CancellationToken cancellationToken = default);
}
