namespace Whyce.Runtime.EventFabric.Workers;

/// <summary>
/// Replay Worker — replays events from the event store through the fabric
/// for projection rebuilds and audit verification.
/// Uses EventReplayService for the actual replay logic.
/// </summary>
public sealed class ReplayWorker
{
    private readonly EventReplayService _replayService;

    public ReplayWorker(EventReplayService replayService)
    {
        _replayService = replayService;
    }

    public async Task ReplayAggregateAsync(Guid aggregateId, Guid correlationId)
    {
        await _replayService.ReplayAsync(aggregateId, correlationId);
    }
}
