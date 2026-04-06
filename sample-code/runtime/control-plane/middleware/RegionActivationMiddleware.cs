using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.ControlPlane.Middleware;

/// <summary>
/// Runtime middleware that gates commands based on region activation status.
/// Rejects commands targeting inactive or halted regions.
/// Non-blocking for operational regions.
/// </summary>
public sealed class RegionActivationMiddleware : IMiddleware
{
    private readonly IRegionActivationProvider _provider;

    public RegionActivationMiddleware(IRegionActivationProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        _provider = provider;
    }

    public async Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next)
    {
        var headers = context.Envelope.Metadata.Headers;

        if (!headers.TryGetValue("x-target-region", out var regionId) || string.IsNullOrWhiteSpace(regionId))
        {
            // No region header — pass through (local-only command)
            return await next(context);
        }

        var status = await _provider.GetRegionStatusAsync(regionId, context.CancellationToken);

        if (status == RegionOperationalStatus.Halted)
        {
            return CommandResult.Fail(
                context.Envelope.CommandId,
                $"Region '{regionId}' is halted. Command rejected.",
                "REGION_HALTED");
        }

        if (status == RegionOperationalStatus.Inactive)
        {
            return CommandResult.Fail(
                context.Envelope.CommandId,
                $"Region '{regionId}' is not yet activated.",
                "REGION_INACTIVE");
        }

        if (status == RegionOperationalStatus.Decommissioned)
        {
            return CommandResult.Fail(
                context.Envelope.CommandId,
                $"Region '{regionId}' has been decommissioned.",
                "REGION_DECOMMISSIONED");
        }

        return await next(context);
    }
}

public enum RegionOperationalStatus
{
    Inactive,
    Canary,
    Active,
    Halted,
    Decommissioned
}

public interface IRegionActivationProvider
{
    Task<RegionOperationalStatus> GetRegionStatusAsync(string regionId, CancellationToken cancellationToken = default);
}
