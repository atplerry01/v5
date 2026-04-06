using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Humancapital.Sponsorship;

public sealed class SponsorshipProjectionHandler
{
    public string ProjectionName => "whyce.structural.humancapital.sponsorship";

    public string[] EventTypes =>
    [
        "whyce.structural.humancapital.sponsorship.created",
        "whyce.structural.humancapital.sponsorship.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISponsorshipViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SponsorshipReadModel
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
