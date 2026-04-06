using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Risk.Review;

public sealed class ReviewProjectionHandler
{
    public string ProjectionName => "whyce.decision.risk.review";

    public string[] EventTypes =>
    [
        "whyce.decision.risk.review.created",
        "whyce.decision.risk.review.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReviewViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReviewReadModel
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
