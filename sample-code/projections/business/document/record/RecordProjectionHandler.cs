using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Document.Record;

public sealed class RecordProjectionHandler
{
    public string ProjectionName => "whyce.business.document.record";

    public string[] EventTypes =>
    [
        "whyce.business.document.record.created",
        "whyce.business.document.record.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRecordViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RecordReadModel
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
