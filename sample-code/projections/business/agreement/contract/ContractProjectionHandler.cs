using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.Contract;

public sealed class ContractProjectionHandler
{
    public string ProjectionName => "whyce.business.agreement.contract";

    public string[] EventTypes =>
    [
        "whyce.business.agreement.contract.created",
        "whyce.business.agreement.contract.updated"
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
