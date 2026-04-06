using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Localization.Timezone;

public sealed class TimezoneProjectionHandler
{
    public string ProjectionName => "whyce.business.localization.timezone";

    public string[] EventTypes =>
    [
        "whyce.business.localization.timezone.created",
        "whyce.business.localization.timezone.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITimezoneViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TimezoneReadModel
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
