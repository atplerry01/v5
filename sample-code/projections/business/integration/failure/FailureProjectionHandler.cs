using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Failure;

public sealed class FailureProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.failure";

    public string[] EventTypes =>
    [
        "whyce.business.integration.failure.created",
        "whyce.business.integration.failure.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IFailureViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new FailureReadModel
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
