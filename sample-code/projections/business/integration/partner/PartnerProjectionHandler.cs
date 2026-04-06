using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Partner;

public sealed class PartnerProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.partner";

    public string[] EventTypes =>
    [
        "whyce.business.integration.partner.created",
        "whyce.business.integration.partner.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPartnerViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PartnerReadModel
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
