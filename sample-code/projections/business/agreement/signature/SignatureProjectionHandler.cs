using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Signature;

public sealed class SignatureProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.signature";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.signature.created",
        "whyce.business.agreement.signature.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISignatureViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SignatureReadModel
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
