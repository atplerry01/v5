using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Search.Synonym;

public sealed class SynonymProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.search.synonym";

    public string[] EventTypes =>
    [
        "whyce.intelligence.search.synonym.created",
        "whyce.intelligence.search.synonym.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISynonymViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SynonymReadModel
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
