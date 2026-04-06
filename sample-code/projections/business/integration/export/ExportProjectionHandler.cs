using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Export;

public sealed class ExportProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.export";

    public string[] EventTypes =>
    [
        "whyce.business.integration.export.created",
        "whyce.business.integration.export.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IExportViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ExportReadModel
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
