using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Notification.Preference;

public sealed class PreferenceProjectionHandler
{
    public string ProjectionName => "whyce.business.notification.preference";

    public string[] EventTypes =>
    [
        "whyce.business.notification.preference.created",
        "whyce.business.notification.preference.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPreferenceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PreferenceReadModel
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
