using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Knowledge.Taxonomy;

public sealed class TaxonomyProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.knowledge.taxonomy";

    public string[] EventTypes =>
    [
        "whyce.intelligence.knowledge.taxonomy.created",
        "whyce.intelligence.knowledge.taxonomy.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITaxonomyViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TaxonomyReadModel
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
