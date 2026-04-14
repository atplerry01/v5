using Whycespace.Projections.Economic.Capital.Vault.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Vault;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Vault;

public sealed class CapitalVaultProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<VaultCreatedEventSchema>,
    IProjectionHandler<VaultSliceCreatedEventSchema>,
    IProjectionHandler<CapitalDepositedEventSchema>,
    IProjectionHandler<CapitalAllocatedToSliceEventSchema>,
    IProjectionHandler<CapitalReleasedFromSliceEventSchema>,
    IProjectionHandler<CapitalWithdrawnEventSchema>
{
    private readonly PostgresProjectionStore<CapitalVaultReadModel> _store;

    public CapitalVaultProjectionHandler(PostgresProjectionStore<CapitalVaultReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            VaultCreatedEventSchema e => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "VaultCreatedEvent", envelope, cancellationToken),
            VaultSliceCreatedEventSchema e => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "VaultSliceCreatedEvent", envelope, cancellationToken),
            CapitalDepositedEventSchema e => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "CapitalDepositedEvent", envelope, cancellationToken),
            CapitalAllocatedToSliceEventSchema e => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "CapitalAllocatedToSliceEvent", envelope, cancellationToken),
            CapitalReleasedFromSliceEventSchema e => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "CapitalReleasedFromSliceEvent", envelope, cancellationToken),
            CapitalWithdrawnEventSchema e => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "CapitalWithdrawnEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CapitalVaultProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(VaultCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "VaultCreatedEvent", null, ct);

    public Task HandleAsync(VaultSliceCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "VaultSliceCreatedEvent", null, ct);

    public Task HandleAsync(CapitalDepositedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "CapitalDepositedEvent", null, ct);

    public Task HandleAsync(CapitalAllocatedToSliceEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "CapitalAllocatedToSliceEvent", null, ct);

    public Task HandleAsync(CapitalReleasedFromSliceEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "CapitalReleasedFromSliceEvent", null, ct);

    public Task HandleAsync(CapitalWithdrawnEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalVaultProjectionReducer.Apply(s, e), "CapitalWithdrawnEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CapitalVaultReadModel, CapitalVaultReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CapitalVaultReadModel { VaultId = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(
            aggregateId,
            state,
            eventTypeName,
            envelope?.EventId ?? Guid.Empty,
            envelope?.SequenceNumber ?? 0,
            envelope?.CorrelationId ?? Guid.Empty,
            ct);
    }
}
