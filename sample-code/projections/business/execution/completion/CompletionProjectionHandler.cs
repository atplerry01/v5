using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Execution.Completion;

public sealed class CompletionProjectionHandler
{
    public string ProjectionName => "whyce.business.execution.completion";

    public string[] EventTypes =>
    [
        "whyce.business.execution.completion.created",
        "whyce.business.execution.completion.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICompletionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CompletionReadModel
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
