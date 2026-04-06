using Whyce.Runtime.EventFabric;

namespace Whyce.Runtime.Projection;

/// <summary>
/// Projection Rebuilder — delegates to EventReplayService for projection rebuilds.
/// </summary>
public sealed class ProjectionRebuilder
{
    private readonly EventReplayService _replayService;

    public ProjectionRebuilder(EventReplayService replayService)
    {
        _replayService = replayService;
    }

    public async Task RebuildAsync(Guid aggregateId, Guid correlationId)
    {
        await _replayService.ReplayAsync(aggregateId, correlationId);
    }
}
