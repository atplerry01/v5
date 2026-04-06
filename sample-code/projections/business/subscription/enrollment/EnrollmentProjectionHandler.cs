using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Subscription.Enrollment;

public sealed class EnrollmentProjectionHandler
{
    public string ProjectionName => "whyce.business.subscription.enrollment";

    public string[] EventTypes =>
    [
        "whyce.business.subscription.enrollment.created",
        "whyce.business.subscription.enrollment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEnrollmentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EnrollmentReadModel
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
