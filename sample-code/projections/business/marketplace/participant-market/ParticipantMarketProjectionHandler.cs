using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Marketplace.ParticipantMarket;

public sealed class ParticipantMarketProjectionHandler
{
    public string ProjectionName => "whyce.business.marketplace.participant-market";

    public string[] EventTypes =>
    [
        "whyce.business.marketplace.participant-market.created",
        "whyce.business.marketplace.participant-market.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IParticipantMarketViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ParticipantMarketReadModel
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
