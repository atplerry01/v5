using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Resource.Facility;

public sealed class FacilityProjectionHandler
{
    public string ProjectionName => "whyce.business.resource.facility";

    public string[] EventTypes =>
    [
        "whyce.business.resource.facility.created",
        "whyce.business.resource.facility.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IFacilityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new FacilityReadModel
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
