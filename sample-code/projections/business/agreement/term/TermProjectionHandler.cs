using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Term;

public sealed class TermProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.term";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.term.created",
        "whyce.business.agreement.term.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITermViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TermReadModel
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
