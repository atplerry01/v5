using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Localization.CurrencyFormat;

public sealed class CurrencyFormatProjectionHandler
{
    public string ProjectionName => "whyce.business.localization.currency-format";

    public string[] EventTypes =>
    [
        "whyce.business.localization.currency-format.created",
        "whyce.business.localization.currency-format.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICurrencyFormatViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CurrencyFormatReadModel
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
