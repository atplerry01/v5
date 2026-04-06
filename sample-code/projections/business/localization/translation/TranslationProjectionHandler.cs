using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Localization.Translation;

public sealed class TranslationProjectionHandler
{
    public string ProjectionName => "whyce.business.localization.translation";

    public string[] EventTypes =>
    [
        "whyce.business.localization.translation.created",
        "whyce.business.localization.translation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITranslationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TranslationReadModel
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
