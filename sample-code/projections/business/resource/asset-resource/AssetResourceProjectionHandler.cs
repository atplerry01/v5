using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Resource.AssetResource;

public sealed class AssetResourceProjectionHandler
{
    public string ProjectionName => "whyce.business.resource.asset-resource";

    public string[] EventTypes =>
    [
        "whyce.business.resource.asset-resource.created",
        "whyce.business.resource.asset-resource.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAssetResourceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AssetResourceReadModel
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
