using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Knowledge.Ontology;

public sealed class OntologyProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.knowledge.ontology";

    public string[] EventTypes =>
    [
        "whyce.intelligence.knowledge.ontology.created",
        "whyce.intelligence.knowledge.ontology.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IOntologyViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new OntologyReadModel
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
