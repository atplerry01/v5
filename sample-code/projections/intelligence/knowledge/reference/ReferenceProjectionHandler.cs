using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Knowledge.Reference;

public sealed class ReferenceProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.knowledge.reference";

    public string[] EventTypes =>
    [
        "whyce.intelligence.knowledge.reference.created",
        "whyce.intelligence.knowledge.reference.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReferenceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReferenceReadModel
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
