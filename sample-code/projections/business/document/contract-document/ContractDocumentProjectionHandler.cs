using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Document.ContractDocument;

public sealed class ContractDocumentProjectionHandler
{
    public string ProjectionName => "whyce.business.document.contract-document";

    public string[] EventTypes =>
    [
        "whyce.business.document.contract-document.created",
        "whyce.business.document.contract-document.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IContractDocumentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ContractDocumentReadModel
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
