using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Humancapital.Stewardship;

public sealed class StewardshipProjectionHandler
{
    public string ProjectionName => "whyce.structural.humancapital.stewardship";

    public string[] EventTypes =>
    [
        "whyce.structural.humancapital.stewardship.created",
        "whyce.structural.humancapital.stewardship.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStewardshipViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StewardshipReadModel
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
