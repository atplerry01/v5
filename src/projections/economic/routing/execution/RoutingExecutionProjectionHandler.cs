using Whycespace.Projections.Economic.Routing.Execution.Reducer;
using Whycespace.Projections.Shared;
using Whycespace.Shared.Contracts.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.EventFabric;
using Whycespace.Shared.Contracts.Events.Economic.Routing.Execution;
using Whycespace.Shared.Contracts.Infrastructure.Projection;
using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Economic.Routing.Execution;

public sealed class RoutingExecutionProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<ExecutionStartedEventSchema>,
    IProjectionHandler<ExecutionCompletedEventSchema>,
    IProjectionHandler<ExecutionFailedEventSchema>,
    IProjectionHandler<ExecutionAbortedEventSchema>
{
    private readonly PostgresProjectionStore<RoutingExecutionReadModel> _store;

    public RoutingExecutionProjectionHandler(PostgresProjectionStore<RoutingExecutionReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            ExecutionStartedEventSchema e   => Project(e.AggregateId, s => RoutingExecutionProjectionReducer.Apply(s, e), "ExecutionStartedEvent", envelope, cancellationToken),
            ExecutionCompletedEventSchema e => Project(e.AggregateId, s => RoutingExecutionProjectionReducer.Apply(s, e), "ExecutionCompletedEvent", envelope, cancellationToken),
            ExecutionFailedEventSchema e    => Project(e.AggregateId, s => RoutingExecutionProjectionReducer.Apply(s, e), "ExecutionFailedEvent", envelope, cancellationToken),
            ExecutionAbortedEventSchema e   => Project(e.AggregateId, s => RoutingExecutionProjectionReducer.Apply(s, e), "ExecutionAbortedEvent", envelope, cancellationToken),
            _ => throw new InvalidOperationException(
                $"RoutingExecutionProjectionHandler received unmatched event: {envelope.Payload.GetType().Name}.")
        };
    }

    public Task HandleAsync(ExecutionStartedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RoutingExecutionProjectionReducer.Apply(s, e), "ExecutionStartedEvent", null, ct);

    public Task HandleAsync(ExecutionCompletedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RoutingExecutionProjectionReducer.Apply(s, e), "ExecutionCompletedEvent", null, ct);

    public Task HandleAsync(ExecutionFailedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RoutingExecutionProjectionReducer.Apply(s, e), "ExecutionFailedEvent", null, ct);

    public Task HandleAsync(ExecutionAbortedEventSchema e, CancellationToken ct = default)
        => Project(e.AggregateId, s => RoutingExecutionProjectionReducer.Apply(s, e), "ExecutionAbortedEvent", null, ct);

    private async Task Project(
        Guid aggregateId,
        Func<RoutingExecutionReadModel, RoutingExecutionReadModel> reduce,
        string eventTypeName,
        IEventEnvelope? envelope,
        CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ??
                    new RoutingExecutionReadModel { ExecutionId = aggregateId };
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
