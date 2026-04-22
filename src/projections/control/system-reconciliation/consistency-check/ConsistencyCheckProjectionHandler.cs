using Whycespace.Projections.Control.SystemReconciliation.ConsistencyCheck.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.ConsistencyCheck;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.SystemReconciliation.ConsistencyCheck;

public sealed class ConsistencyCheckProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ConsistencyCheckInitiatedEventSchema>,
    IProjectionHandler<ConsistencyCheckCompletedEventSchema>
{
    private readonly PostgresProjectionStore<ConsistencyCheckReadModel> _store;

    public ConsistencyCheckProjectionHandler(PostgresProjectionStore<ConsistencyCheckReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ConsistencyCheckInitiatedEventSchema e  => Project(e.AggregateId, s => ConsistencyCheckProjectionReducer.Apply(s, e), "ConsistencyCheckInitiatedEvent",  envelope, cancellationToken),
            ConsistencyCheckCompletedEventSchema e  => Project(e.AggregateId, s => ConsistencyCheckProjectionReducer.Apply(s, e), "ConsistencyCheckCompletedEvent",  envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ConsistencyCheckProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ConsistencyCheckInitiatedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConsistencyCheckProjectionReducer.Apply(s, e), "ConsistencyCheckInitiatedEvent", null, ct);

    public Task HandleAsync(ConsistencyCheckCompletedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ConsistencyCheckProjectionReducer.Apply(s, e), "ConsistencyCheckCompletedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ConsistencyCheckReadModel, ConsistencyCheckReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ConsistencyCheckReadModel { CheckId = aggregateId };
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
