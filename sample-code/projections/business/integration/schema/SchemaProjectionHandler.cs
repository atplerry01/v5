using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Schema;

public sealed class SchemaProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.schema";

    public string[] EventTypes =>
    [
        "whyce.business.integration.schema.created",
        "whyce.business.integration.schema.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISchemaViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SchemaReadModel
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
