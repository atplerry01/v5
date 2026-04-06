using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Reserve;

public sealed class ReserveProjectionHandler
{
    public string ProjectionName => "whyce.economic.capital.reserve";

    public string[] EventTypes =>
    [
        "whyce.economic.capital.reserve.created",
        "whyce.economic.capital.reserve.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReserveViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReserveReadModel
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
