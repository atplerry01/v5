using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Mapping;

public sealed class MappingProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.mapping";

    public string[] EventTypes =>
    [
        "whyce.business.integration.mapping.created",
        "whyce.business.integration.mapping.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IMappingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new MappingReadModel
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
