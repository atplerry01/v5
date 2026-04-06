using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Renewal;

public sealed class RenewalProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.renewal";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.renewal.created",
        "whyce.business.agreement.renewal.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRenewalViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RenewalReadModel
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
