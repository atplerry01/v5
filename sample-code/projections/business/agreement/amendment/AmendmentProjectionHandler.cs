using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Amendment;

public sealed class AmendmentProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.amendment";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.amendment.created",
        "whyce.business.agreement.amendment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAmendmentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AmendmentReadModel
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
