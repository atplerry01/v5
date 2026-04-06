using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Import;

public sealed class ImportProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.import";

    public string[] EventTypes =>
    [
        "whyce.business.integration.import.created",
        "whyce.business.integration.import.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IImportViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ImportReadModel
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
