using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Vault;

public sealed class VaultProjectionHandler
{
    public string ProjectionName => "whyce.economic.capital.vault";

    public string[] EventTypes =>
    [
        "whyce.economic.capital.vault.created",
        "whyce.economic.capital.vault.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IVaultViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new VaultReadModel
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
