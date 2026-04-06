using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Validity;

public sealed class ValidityProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.validity";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.validity.created",
        "whyce.business.agreement.validity.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IValidityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ValidityReadModel
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
