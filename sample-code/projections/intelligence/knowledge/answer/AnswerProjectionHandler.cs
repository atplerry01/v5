using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Knowledge.Answer;

public sealed class AnswerProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.knowledge.answer";

    public string[] EventTypes =>
    [
        "whyce.intelligence.knowledge.answer.created",
        "whyce.intelligence.knowledge.answer.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAnswerViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AnswerReadModel
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
