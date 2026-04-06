using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Capital;

public sealed class CapitalProjectionHandler
{
    public string ProjectionName => "whyce.economic.capital.capital";

    public string[] EventTypes =>
    [
        "whyce.economic.capital.capital.created",
        "whyce.economic.capital.capital.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICapitalViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CapitalReadModel
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
