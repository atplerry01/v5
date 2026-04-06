using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Job;

public sealed class JobProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.job";

    public string[] EventTypes =>
    [
        "whyce.business.integration.job.created",
        "whyce.business.integration.job.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IJobViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new JobReadModel
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
