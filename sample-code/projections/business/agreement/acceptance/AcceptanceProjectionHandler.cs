using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Acceptance;

public sealed class AcceptanceProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.acceptance";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.acceptance.created",
        "whyce.business.agreement.acceptance.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAcceptanceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AcceptanceReadModel
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
