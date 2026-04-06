using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Asset;

public sealed class AssetProjectionHandler
{
    public string ProjectionName => "whyce.economic.capital.asset";

    public string[] EventTypes =>
    [
        "whyce.economic.capital.asset.created",
        "whyce.economic.capital.asset.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAssetViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AssetReadModel
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
