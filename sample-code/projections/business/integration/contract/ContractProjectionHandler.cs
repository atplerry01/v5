using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Contract;

public sealed class ContractProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.contract";

    public string[] EventTypes =>
    [
        "whyce.business.integration.contract.created",
        "whyce.business.integration.contract.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IContractViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ContractReadModel
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
