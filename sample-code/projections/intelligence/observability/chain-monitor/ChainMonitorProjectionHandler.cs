using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Observability.ChainMonitor;

public sealed class ChainMonitorProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.observability.chain-monitor";

    public string[] EventTypes =>
    [
        "whyce.intelligence.observability.chain-monitor.created",
        "whyce.intelligence.observability.chain-monitor.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IChainMonitorViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ChainMonitorReadModel
        {
            Id = @event.AggregateId.ToString(),
            Status = "Active",
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
    }
}
