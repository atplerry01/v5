using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Transformation;

public sealed class TransformationProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.transformation";

    public string[] EventTypes =>
    [
        "whyce.business.integration.transformation.created",
        "whyce.business.integration.transformation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITransformationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TransformationReadModel
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
