using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Localization.Locale;

public sealed class LocaleProjectionHandler
{
    public string ProjectionName => "whyce.business.localization.locale";

    public string[] EventTypes =>
    [
        "whyce.business.localization.locale.created",
        "whyce.business.localization.locale.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ILocaleViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new LocaleReadModel
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
