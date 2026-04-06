using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Risk.Assessment;

public sealed class AssessmentProjectionHandler
{
    public string ProjectionName => "whyce.decision.risk.assessment";

    public string[] EventTypes =>
    [
        "whyce.decision.risk.assessment.created",
        "whyce.decision.risk.assessment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAssessmentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AssessmentReadModel
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
