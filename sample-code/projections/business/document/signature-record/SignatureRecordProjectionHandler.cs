using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Document.SignatureRecord;

public sealed class SignatureRecordProjectionHandler
{
    public string ProjectionName => "whyce.business.document.signature-record";

    public string[] EventTypes =>
    [
        "whyce.business.document.signature-record.created",
        "whyce.business.document.signature-record.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISignatureRecordViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SignatureRecordReadModel
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
