using Whyce.Projections.Operational.Sandbox.Todo.Reducer;
using Whyce.Projections.Shared;
using Whyce.Shared.Contracts.Operational.Sandbox.Todo;
using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Events.Operational.Sandbox.Todo;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Projection;

namespace Whyce.Projections.Operational.Sandbox.Todo.Item;

public sealed class TodoProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<TodoCreatedEventSchema>,
    IProjectionHandler<TodoUpdatedEventSchema>,
    IProjectionHandler<TodoCompletedEventSchema>
{
    private readonly PostgresProjectionStore<TodoReadModel> _store;

    public TodoProjectionHandler(PostgresProjectionStore<TodoReadModel> store) => _store = store;

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        return envelope.Payload switch
        {
            TodoCreatedEventSchema e => Project(e.AggregateId, s => TodoProjectionReducer.Apply(s, e), "TodoCreatedEvent", envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            TodoUpdatedEventSchema e => Project(e.AggregateId, s => TodoProjectionReducer.Apply(s, e), "TodoUpdatedEvent", envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            TodoCompletedEventSchema e => Project(e.AggregateId, s => TodoProjectionReducer.Apply(s, e), "TodoCompletedEvent", envelope.EventId, envelope.SequenceNumber, envelope.CorrelationId, cancellationToken),
            _ => throw new InvalidOperationException(
                $"TodoProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}. " +
                $"EventId={envelope.EventId}, EventType={envelope.EventType}.")
        };
    }

    public async Task HandleAsync(TodoCreatedEventSchema e, CancellationToken ct = default) => await Project(e.AggregateId, s => TodoProjectionReducer.Apply(s, e), "TodoCreatedEvent", Guid.Empty, 0, Guid.Empty, ct);
    public async Task HandleAsync(TodoUpdatedEventSchema e, CancellationToken ct = default) => await Project(e.AggregateId, s => TodoProjectionReducer.Apply(s, e), "TodoUpdatedEvent", Guid.Empty, 0, Guid.Empty, ct);
    public async Task HandleAsync(TodoCompletedEventSchema e, CancellationToken ct = default) => await Project(e.AggregateId, s => TodoProjectionReducer.Apply(s, e), "TodoCompletedEvent", Guid.Empty, 0, Guid.Empty, ct);

    private async Task Project(Guid aggregateId, Func<TodoReadModel, TodoReadModel> reduce, string eventType, Guid eventId, long eventVersion, Guid correlationId, CancellationToken ct)
    {
        var state = await _store.LoadAsync(aggregateId, ct) ?? new TodoReadModel { Id = aggregateId };
        state = reduce(state);
        await _store.UpsertAsync(aggregateId, state, eventType, eventId, eventVersion, correlationId, ct);
    }
}
