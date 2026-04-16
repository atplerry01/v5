using Whycespace.Projections.Economic.Vault.Account.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Vault.Account;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Vault.Account;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Vault.Account;

/// <summary>
/// Single projection handler for all vault/account events. Materializes the
/// VaultAccountReadModel from the event stream (Funded → Invested → Debited → Credited → SpvProfit).
/// </summary>
public sealed class VaultAccountProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<VaultAccountCreatedEventSchema>,
    IProjectionHandler<VaultFundedEventSchema>,
    IProjectionHandler<CapitalAllocatedToSliceEventSchema>,
    IProjectionHandler<SpvProfitReceivedEventSchema>,
    IProjectionHandler<VaultDebitedEventSchema>,
    IProjectionHandler<VaultCreditedEventSchema>
{
    private readonly PostgresProjectionStore<VaultAccountReadModel> _store;

    public VaultAccountProjectionHandler(PostgresProjectionStore<VaultAccountReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            VaultAccountCreatedEventSchema e => Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "VaultAccountCreatedEvent", envelope, cancellationToken),
            VaultFundedEventSchema e => Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "VaultFundedEvent", envelope, cancellationToken),
            CapitalAllocatedToSliceEventSchema e => Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "CapitalAllocatedToSliceEvent", envelope, cancellationToken),
            SpvProfitReceivedEventSchema e => Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "SpvProfitReceivedEvent", envelope, cancellationToken),
            VaultDebitedEventSchema e => Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "VaultDebitedEvent", envelope, cancellationToken),
            VaultCreditedEventSchema e => Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "VaultCreditedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"VaultAccountProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public async Task HandleAsync(VaultAccountCreatedEventSchema e, CancellationToken ct = default)
        => await Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "VaultAccountCreatedEvent", null, ct);

    public async Task HandleAsync(VaultFundedEventSchema e, CancellationToken ct = default)
        => await Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "VaultFundedEvent", null, ct);

    public async Task HandleAsync(CapitalAllocatedToSliceEventSchema e, CancellationToken ct = default)
        => await Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "CapitalAllocatedToSliceEvent", null, ct);

    public async Task HandleAsync(SpvProfitReceivedEventSchema e, CancellationToken ct = default)
        => await Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "SpvProfitReceivedEvent", null, ct);

    public async Task HandleAsync(VaultDebitedEventSchema e, CancellationToken ct = default)
        => await Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "VaultDebitedEvent", null, ct);

    public async Task HandleAsync(VaultCreditedEventSchema e, CancellationToken ct = default)
        => await Project(e.AggregateId, s => VaultAccountProjectionReducer.Apply(s, e), "VaultCreditedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<VaultAccountReadModel, VaultAccountReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new VaultAccountReadModel { VaultAccountId = aggregateId };
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
