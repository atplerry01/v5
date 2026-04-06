using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Execution.Setup;

public sealed class SetupProjectionHandler
{
    public string ProjectionName => "whyce.business.execution.setup";

    public string[] EventTypes =>
    [
        "whyce.business.execution.setup.created",
        "whyce.business.execution.setup.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISetupViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SetupReadModel
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
