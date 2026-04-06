using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Binding;

public sealed class BindingProjectionHandler
{
    public string ProjectionName => "whyce.economic.capital.binding";

    public string[] EventTypes =>
    [
        "whyce.economic.capital.binding.created",
        "whyce.economic.capital.binding.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IBindingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new BindingReadModel
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
