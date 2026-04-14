using Whycespace.Projections.Economic.Capital.Account.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Capital.Account;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Account;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Capital.Account;

public sealed class CapitalAccountProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<CapitalAccountOpenedEventSchema>,
    IProjectionHandler<CapitalFundedEventSchema>,
    IProjectionHandler<AccountCapitalAllocatedEventSchema>,
    IProjectionHandler<AccountCapitalReservedEventSchema>,
    IProjectionHandler<AccountReservationReleasedEventSchema>,
    IProjectionHandler<CapitalAccountFrozenEventSchema>,
    IProjectionHandler<CapitalAccountClosedEventSchema>
{
    private readonly PostgresProjectionStore<CapitalAccountReadModel> _store;

    public CapitalAccountProjectionHandler(PostgresProjectionStore<CapitalAccountReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            CapitalAccountOpenedEventSchema e => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "CapitalAccountOpenedEvent", envelope, cancellationToken),
            CapitalFundedEventSchema e => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "CapitalFundedEvent", envelope, cancellationToken),
            AccountCapitalAllocatedEventSchema e => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "AccountCapitalAllocatedEvent", envelope, cancellationToken),
            AccountCapitalReservedEventSchema e => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "AccountCapitalReservedEvent", envelope, cancellationToken),
            AccountReservationReleasedEventSchema e => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "AccountReservationReleasedEvent", envelope, cancellationToken),
            CapitalAccountFrozenEventSchema e => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "CapitalAccountFrozenEvent", envelope, cancellationToken),
            CapitalAccountClosedEventSchema e => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "CapitalAccountClosedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CapitalAccountProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(CapitalAccountOpenedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "CapitalAccountOpenedEvent", null, ct);

    public Task HandleAsync(CapitalFundedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "CapitalFundedEvent", null, ct);

    public Task HandleAsync(AccountCapitalAllocatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "AccountCapitalAllocatedEvent", null, ct);

    public Task HandleAsync(AccountCapitalReservedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "AccountCapitalReservedEvent", null, ct);

    public Task HandleAsync(AccountReservationReleasedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "AccountReservationReleasedEvent", null, ct);

    public Task HandleAsync(CapitalAccountFrozenEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "CapitalAccountFrozenEvent", null, ct);

    public Task HandleAsync(CapitalAccountClosedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CapitalAccountProjectionReducer.Apply(s, e), "CapitalAccountClosedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CapitalAccountReadModel, CapitalAccountReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CapitalAccountReadModel { AccountId = aggregateId };
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
