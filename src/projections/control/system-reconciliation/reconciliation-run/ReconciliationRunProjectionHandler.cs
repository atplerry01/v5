using Whycespace.Projections.Control.SystemReconciliation.ReconciliationRun.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.ReconciliationRun;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.SystemReconciliation.ReconciliationRun;

public sealed class ReconciliationRunProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ReconciliationRunScheduledEventSchema>,
    IProjectionHandler<ReconciliationRunStartedEventSchema>,
    IProjectionHandler<ReconciliationRunCompletedEventSchema>,
    IProjectionHandler<ReconciliationRunAbortedEventSchema>
{
    private readonly PostgresProjectionStore<ReconciliationRunReadModel> _store;

    public ReconciliationRunProjectionHandler(PostgresProjectionStore<ReconciliationRunReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ReconciliationRunScheduledEventSchema e  => Project(e.AggregateId, s => ReconciliationRunProjectionReducer.Apply(s, e), "ReconciliationRunScheduledEvent",  envelope, cancellationToken),
            ReconciliationRunStartedEventSchema e    => Project(e.AggregateId, s => ReconciliationRunProjectionReducer.Apply(s, e), "ReconciliationRunStartedEvent",    envelope, cancellationToken),
            ReconciliationRunCompletedEventSchema e  => Project(e.AggregateId, s => ReconciliationRunProjectionReducer.Apply(s, e), "ReconciliationRunCompletedEvent",  envelope, cancellationToken),
            ReconciliationRunAbortedEventSchema e    => Project(e.AggregateId, s => ReconciliationRunProjectionReducer.Apply(s, e), "ReconciliationRunAbortedEvent",    envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ReconciliationRunProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ReconciliationRunScheduledEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ReconciliationRunProjectionReducer.Apply(s, e), "ReconciliationRunScheduledEvent", null, ct);

    public Task HandleAsync(ReconciliationRunStartedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ReconciliationRunProjectionReducer.Apply(s, e), "ReconciliationRunStartedEvent", null, ct);

    public Task HandleAsync(ReconciliationRunCompletedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ReconciliationRunProjectionReducer.Apply(s, e), "ReconciliationRunCompletedEvent", null, ct);

    public Task HandleAsync(ReconciliationRunAbortedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ReconciliationRunProjectionReducer.Apply(s, e), "ReconciliationRunAbortedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ReconciliationRunReadModel, ReconciliationRunReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ReconciliationRunReadModel { RunId = aggregateId };
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
