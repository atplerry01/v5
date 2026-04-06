using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Clause;

public sealed class ClauseProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.clause";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.clause.created",
        "whyce.business.agreement.clause.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IClauseViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ClauseReadModel
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
