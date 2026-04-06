using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.Profile;

public sealed class ProfileProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.profile";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.profile.created",
        "whyce.trust.identity.profile.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IProfileViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ProfileReadModel
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
