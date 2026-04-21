using Whycespace.Projections.Business.Agreement.PartyGovernance.Counterparty.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Business.Agreement.PartyGovernance.Counterparty;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Agreement.PartyGovernance.Counterparty;

public sealed class CounterpartyProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<CounterpartyCreatedEventSchema>,
    IProjectionHandler<CounterpartySuspendedEventSchema>,
    IProjectionHandler<CounterpartyTerminatedEventSchema>
{
    private readonly PostgresProjectionStore<CounterpartyReadModel> _store;

    public CounterpartyProjectionHandler(PostgresProjectionStore<CounterpartyReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            CounterpartyCreatedEventSchema e    => Project(e.AggregateId, s => CounterpartyProjectionReducer.Apply(s, e), "CounterpartyCreatedEvent",    envelope, cancellationToken),
            CounterpartySuspendedEventSchema e  => Project(e.AggregateId, s => CounterpartyProjectionReducer.Apply(s, e), "CounterpartySuspendedEvent",  envelope, cancellationToken),
            CounterpartyTerminatedEventSchema e => Project(e.AggregateId, s => CounterpartyProjectionReducer.Apply(s, e), "CounterpartyTerminatedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"CounterpartyProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(CounterpartyCreatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CounterpartyProjectionReducer.Apply(s, e), "CounterpartyCreatedEvent", null, ct);

    public Task HandleAsync(CounterpartySuspendedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CounterpartyProjectionReducer.Apply(s, e), "CounterpartySuspendedEvent", null, ct);

    public Task HandleAsync(CounterpartyTerminatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => CounterpartyProjectionReducer.Apply(s, e), "CounterpartyTerminatedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<CounterpartyReadModel, CounterpartyReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new CounterpartyReadModel { CounterpartyId = aggregateId };
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
