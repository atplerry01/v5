using Whycespace.Projections.Control.Scheduling.ExecutionControl.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Control.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Control.Scheduling.ExecutionControl;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Control.Scheduling.ExecutionControl;

public sealed class ExecutionControlProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ExecutionControlSignalIssuedEventSchema>,
    IProjectionHandler<ExecutionControlSignalOutcomeRecordedEventSchema>
{
    private readonly PostgresProjectionStore<ExecutionControlReadModel> _store;

    public ExecutionControlProjectionHandler(PostgresProjectionStore<ExecutionControlReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ExecutionControlSignalIssuedEventSchema e          => Project(e.AggregateId, s => ExecutionControlProjectionReducer.Apply(s, e), "ExecutionControlSignalIssuedEvent",          envelope, cancellationToken),
            ExecutionControlSignalOutcomeRecordedEventSchema e => Project(e.AggregateId, s => ExecutionControlProjectionReducer.Apply(s, e), "ExecutionControlSignalOutcomeRecordedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"ExecutionControlProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ExecutionControlSignalIssuedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ExecutionControlProjectionReducer.Apply(s, e), "ExecutionControlSignalIssuedEvent", null, ct);

    public Task HandleAsync(ExecutionControlSignalOutcomeRecordedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => ExecutionControlProjectionReducer.Apply(s, e), "ExecutionControlSignalOutcomeRecordedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<ExecutionControlReadModel, ExecutionControlReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new ExecutionControlReadModel { ControlId = aggregateId };
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
